using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GTFS;
using GTFS.Entities;
using GTFS.Entities.Enumerations;
using GTFS_Explorer.BackEnd.Utilities;
using GTFS_Explorer.Core.Enums;
using GTFS_Explorer.Core.Models.Structs;
using Nixill.Collections.Grid;
using Nixill.GTFS;
using NodaTime;
using NodaTime.Text;

namespace GTFS_Explorer.BackEnd.Lookups
{
    public static class RouteLookups
    {
        private static readonly DurationPattern FrequencyParser = DurationPattern.CreateWithInvariantCulture("HH:mm:ss");

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="serviceIds">The IDs of the days to retrieve.</param>
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, List<string> serviceIds) =>
          GetSchedule(feed, route, dir, serviceIds, TimepointFinder.GetTimepointStrategy(feed));

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        ///   <para>This overload is deprecated; <c>serviceId</c> should be
        ///     replaced with <c>new List&lt;string&gt; { serviceId }</c>.
        ///   </para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="serviceId">The ID of the service to retrieve.</param>
        [Obsolete("Change `serviceId` to `new List<string> { serviceId }`.")]
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, string serviceId) =>
          GetSchedule(feed, route, dir, new List<string> { serviceId }, TimepointFinder.GetTimepointStrategy(feed));

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="date">The date to retrieve.</param>
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, LocalDate date) =>
          GetSchedule(feed, route, dir, ServicesOn(feed, date).Item1);

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="serviceIds">The IDs of the days to retrieve.</param>
        /// <param name="strat">
        ///   The timepoint strategy, a value returned by
        ///   <c>TimepointFinder.GetTimepointStrategy()</c>.
        /// </param>
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, List<string> serviceIds, TimepointStrategy strat)
        {
            var stops = ScheduleBuilder.GetScheduleHeader(feed, route, dir, strat);
            var times = ScheduleBuilder.GetSortTimes(feed, route, dir, stops);
            var sched = ScheduleBuilder.GetSchedule(feed, route, dir, serviceIds, stops, times);
            return GridifySchedule(feed, sched);
        }

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        ///   <para>This overload is deprecated; <c>serviceId</c> should be
        ///     replaced with <c>new List&lt;string&gt; { serviceId }</c>.
        ///   </para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="serviceId">The ID of the service to retrieve.</param>
        /// <param name="strat">
        ///   The timepoint strategy, a value returned by
        ///   <c>TimepointFinder.GetTimepointStrategy()</c>.
        /// </param>
        [Obsolete("Change `serviceId` to `new List<string> { serviceId }`.")]
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, string serviceId, TimepointStrategy strat) =>
          GetSchedule(feed, route, dir, new List<string> { serviceId }, strat);

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="date">The date to retrieve.</param>
        /// <param name="strat">
        ///   The timepoint strategy, a value returned by
        ///   <c>TimepointFinder.GetTimepointStrategy()</c>.
        /// </param>
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, LocalDate date, TimepointStrategy strat) =>
          GetSchedule(feed, route, dir, ServicesOn(feed, date).Item1, strat);

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="serviceIds">The IDs of the days to retrieve.</param>
        /// <param name="stopOrder">The order of stops to use.</param>
        /// <param name="sortTimes">The sort times of the trips.</param>
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, List<string> serviceIds, List<string> stopOrder, Dictionary<string, int> sortTimes) =>
          GridifySchedule(feed, ScheduleBuilder.GetSchedule(feed, route, dir, serviceIds, stopOrder, sortTimes));

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        ///   <para>This overload is deprecated; <c>serviceId</c> should be
        ///     replaced with <c>new List&lt;string&gt; { serviceId }</c>.
        ///   </para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="serviceId">The ID of the service to retrieve.</param>
        /// <param name="stopOrder">The order of stops to use.</param>
        /// <param name="sortTimes">The sort times of the trips.</param>
        [Obsolete("Change `serviceId` to `new List<string> { serviceId }`.")]
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, string serviceId, List<string> stopOrder, Dictionary<string, int> sortTimes) =>
          GridifySchedule(feed, ScheduleBuilder.GetSchedule(feed, route, dir, new List<string> { serviceId }, stopOrder, sortTimes));

        /// <summary>
        ///   <para>Returns a transit schedule in grid form.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The returned schedule will have stop IDs along the first
        ///     row, trip IDs in the first column, and times of when that trip
        ///     reaches that stop in the rest of the grid.</para>
        /// </remarks>
        /// <param name="feed">The GTFS feed to use.</param>
        /// <param name="route">The ID of the route to retrieve.</param>
        /// <param name="dir">
        ///   Which direction of the route should be scheduled.
        /// </param>
        /// <param name="date">The date to retrieve.</param>
        /// <param name="stopOrder">The order of stops to use.</param>
        /// <param name="sortTimes">The sort times of the trips.</param>
        public static Grid<string> GetSchedule(GTFSFeed feed, string route, DirectionType? dir, LocalDate date, List<string> stopOrder, Dictionary<string, int> sortTimes) =>
          GridifySchedule(feed, ScheduleBuilder.GetSchedule(feed, route, dir, ServicesOn(feed, date).Item1, stopOrder, sortTimes));

        private static Grid<string> GridifySchedule(GTFSFeed feed, Tuple<List<string>, List<Tuple<string, Dictionary<string, TimeOfDay>>>> sched)
        {
            Grid<string> ret = new Grid<string>();

            // Add the stops row
            var stops = sched.Item1;
            stops.Insert(0, "");
            ret.AddRow(stops);

            // Prepare a frequencies row
            List<string> freqRow = Enumerable.Repeat("", stops.Count).ToList();

            // Now add the trips
            var trips = sched.Item2;
            foreach (var trip in trips)
            {
                List<string> tripRow = new List<string>();
                // Add the times at each stop
                for (int i = 0; i < stops.Count; i++)
                {
                    string stop = stops[i];
                    if (stop == "")
                    {
                        tripRow.Add(trip.Item1);
                    }
                    else
                    {
                        if (stop == stops[1] && i != 1)
                        {
                            int time = trip.Item2[""].TotalSeconds % 86400;
                            string timeString = NodaTime.LocalTime.FromSecondsSinceMidnight(time).ToString("HH:mm", DateTimeFormatInfo.InvariantInfo);
                            tripRow.Add(timeString);
                        }
                        else if (trip.Item2.ContainsKey(stop))
                        {
                            int time = trip.Item2[stop].TotalSeconds % 86400;
                            string timeString = NodaTime.LocalTime.FromSecondsSinceMidnight(time).ToString("HH:mm", DateTimeFormatInfo.InvariantInfo);
                            tripRow.Add(timeString);
                        }
                        else
                        {
                            tripRow.Add("");
                        }
                    }
                }

                ret.AddRow(tripRow);

                // Also frequency-based trips
                var freqs = feed.Frequencies.Where(x => x.TripId == trip.Item1);

                foreach (var freq in freqs)
                {
                    string freqDesc = "";
                    if (!(freq.ExactTimes.HasValue && freq.ExactTimes.Value))
                    {
                        freqDesc = "approximately ";
                    }

                    freqDesc += "every ";

                    int headway = int.Parse(freq.HeadwaySecs);
                    int secs, mins, hrs;

                    if (headway < 300)
                    {
                        secs = headway % 60;
                    }
                    else
                    {
                        secs = 0;
                        headway += 30; // For rounding
                    }

                    headway /= 60;

                    mins = headway % 60;
                    hrs = headway / 60;

                    if (hrs > 1)
                    {
                        freqDesc += $" {hrs} hours";
                    }
                    else if (hrs == 1)
                    {
                        freqDesc += " 1 hour";
                    }

                    if (mins > 1)
                    {
                        freqDesc += $" {mins} minutes";
                    }
                    else if (mins == 1)
                    {
                        freqDesc += " 1 minute";
                    }

                    if (secs > 1)
                    {
                        freqDesc += $" {secs} seconds";
                    }
                    else if (secs == 1)
                    {
                        freqDesc += " 1 second";
                    }

                    string startTime = ExtraUtils.RemoveSeconds(freq.StartTime);
                    string endTime = ExtraUtils.RemoveSeconds(freq.EndTime);

                    freqDesc += $" from {startTime} to {endTime}";

                    freqRow[1] = freqDesc;
                    ret.AddRow(freqRow);
                }
            }

            return ret;
        }

        /// <summary>
        ///   Returns whether or not the Route has any trips or any stop
        ///   times.
        /// </summary>
        /// <param name="feed">The GTFS feed to check.</param>
        /// <param name="string">The ID of the route to check.</param>
        public static bool HasAnyService(GTFSFeed feed, string route)
        {
            // First see if it has any trips
            var trips =
              from trip in feed.Trips
              where trip.RouteId == route
              select trip;

            // If so, let's see if any of them have any stop times
            foreach (Trip theTrip in trips)
            {
                var stopTimes =
                  from time in feed.StopTimes
                  where time.TripId == theTrip.Id
                  select time;

                if (stopTimes.Any()) return true;
            }

            return false;
        }

        /// <summary>
        ///   Returns a <c>Tuple</c> which contains a <c>List</c> of all
        ///   the service IDs active on a given day, and a <c>bool</c>
        ///   indicating whether or not that's a generic list.
        /// </summary>
        /// <remarks>
        ///   If no services run on a given day, an empty list is returned.
        /// </remarks>
        /// <param name="feed">The GTFS feed to check.</param>
        /// <param name="date">The date to check.</param>
        public static Tuple<List<string>, bool> ServicesOn(GTFSFeed feed, LocalDate date)
        {
            DateRange range = Lists.GetServiceRange(feed);
            DateTime dt = date.ToDateTimeUnspecified();

            if (date >= range.Min && date <= range.Max)
            {
                var ret = from cal in feed.Calendars
                          where cal.StartDate <= dt
                              && cal.EndDate >= dt
                              && date.DayOfWeek switch
                              {
                                  IsoDayOfWeek.Monday => cal.Monday,
                                  IsoDayOfWeek.Tuesday => cal.Tuesday,
                                  IsoDayOfWeek.Wednesday => cal.Wednesday,
                                  IsoDayOfWeek.Thursday => cal.Thursday,
                                  IsoDayOfWeek.Friday => cal.Friday,
                                  IsoDayOfWeek.Saturday => cal.Saturday,
                                  IsoDayOfWeek.Sunday => cal.Sunday,
                                  _ => false
                              }
                          select cal.ServiceId;

                ret = ret.Except(from cald in feed.CalendarDates
                                 where cald.Date == dt && cald.ExceptionType == ExceptionType.Removed
                                 select cald.ServiceId);

                ret = ret.Union(from cald in feed.CalendarDates
                                where cald.Date == dt && cald.ExceptionType == ExceptionType.Added
                                select cald.ServiceId);

                return new Tuple<List<string>, bool>(ret.Distinct().ToList(), false);
            }
            else
            {
                var ret = from cal in feed.Calendars
                          where date.DayOfWeek switch
                          {
                              IsoDayOfWeek.Monday => cal.Monday,
                              IsoDayOfWeek.Tuesday => cal.Tuesday,
                              IsoDayOfWeek.Wednesday => cal.Wednesday,
                              IsoDayOfWeek.Thursday => cal.Thursday,
                              IsoDayOfWeek.Friday => cal.Friday,
                              IsoDayOfWeek.Saturday => cal.Saturday,
                              IsoDayOfWeek.Sunday => cal.Sunday,
                              _ => false
                          }
                          select cal.ServiceId;

                return new Tuple<List<string>, bool>(ret.Distinct().ToList(), true);
            }
        }

        public static Dictionary<DirectionType?, RouteStats> GetRouteStats(GTFSFeed feed, LocalDate date, string route)
        {
            Dictionary<DirectionType?, RouteStats> ret = new Dictionary<DirectionType?, RouteStats>();

            var services = ServicesOn(feed, date);
            var allTrips = feed.Trips.Where(x => x.RouteId == route && services.Item1.Contains(x.ServiceId));
            var directions = allTrips.Select(x => x.Direction).Distinct();

            foreach (DirectionType? dir in directions)
            {
                var dirTrips = allTrips.Where(x => x.Direction == dir);

                List<string> firstStops = new List<string>();
                List<string> lastStops = new List<string>();

                var rs = new RouteStats()
                {
                    StartStops = new List<string>(),
                    EndStops = new List<string>(),
                    TotalTrips = 0,
                    AverageTrip = Duration.FromSeconds(0),
                    StartTime = Duration.MaxValue,
                    EndTime = Duration.FromSeconds(0),
                    ShortestTrip = Duration.MaxValue,
                    LongestTrip = Duration.FromSeconds(0)
                };

                foreach (Trip t in dirTrips)
                {
                    var stops = feed.StopTimes.Where(x => x.TripId == t.Id).OrderBy(x => x.ArrivalTime);
                    var firstStop = stops.First();
                    var lastStop = stops.Last();

                    firstStops.Add(firstStop.StopId);
                    lastStops.Add(lastStop.StopId);

                    var firstTime = Duration.FromSeconds(firstStop.DepartureTime.Value.TotalSeconds);
                    var lastTime = Duration.FromSeconds(lastStop.ArrivalTime.Value.TotalSeconds);
                    var length = lastTime - firstTime;

                    if (rs.StartTime > firstTime) rs.StartTime = firstTime;
                    if (rs.EndTime < lastTime) rs.EndTime = lastTime;
                    if (rs.ShortestTrip > length) rs.ShortestTrip = length;
                    if (rs.LongestTrip < length) rs.LongestTrip = length;

                    rs.AverageTrip += length;
                    rs.TotalTrips += 1;

                    // Don't forget about frequencies
                    var freqs = feed.Frequencies.Where(x => x.TripId == t.Id);
                    foreach (Frequency f in freqs)
                    {
                        var firstTimeF = FrequencyParser.Parse(f.StartTime).Value;
                        var lastTimeF = FrequencyParser.Parse(f.EndTime).Value;

                        lastTimeF += (lastTime - firstTime);

                        if (rs.StartTime > firstTimeF) rs.StartTime = firstTimeF;
                        if (rs.EndTime < lastTimeF) rs.EndTime = lastTimeF;
                    }
                }

                rs.AverageTrip /= rs.TotalTrips;
                rs.StartStops.AddRange(firstStops.Distinct().Select(x => feed.Stops.Get(x).Name));
                rs.EndStops.AddRange(lastStops.Distinct().Select(x => feed.Stops.Get(x).Name));
                ret[dir] = rs;

                rs.GenericDay = services.Item2;
            }

            return ret;
        }

        public static IEnumerable<Tuple<Stop, bool>> GetRouteStops(GTFSFeed feed, string routeId, DirectionType? dir) => GetRouteStops(feed, routeId, dir, TimepointFinder.GetTimepointStrategy(feed));
        public static IEnumerable<Tuple<Stop, bool>> GetRouteStops(GTFSFeed feed, string routeId, DirectionType? dir, TimepointStrategy strat)
        {
            var stops = StopLister.GetStopOrder(feed, routeId, dir);
            List<string> timepoints = ScheduleBuilder.GetScheduleHeader(feed, routeId, dir, strat);

            foreach (Stop stop in stops)
            {
                if (timepoints.Contains(stop.Id))
                {
                    yield return new Tuple<Stop, bool>(stop, true);
                }
                else
                {
                    yield return new Tuple<Stop, bool>(stop, false);
                }
            }
        }
    }
}