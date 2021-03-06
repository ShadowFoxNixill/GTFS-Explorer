﻿using GTFS.Entities;
using GTFS.Entities.Enumerations;
using GTFS_Explorer.Core.Enums;
using System.Collections.Generic;
using Nixill.Collections.Grid;
using NodaTime;
using GTFS_Explorer.Core.Models.Structs;
using System;

namespace GTFS_Explorer.Core.Interfaces.RepoInterfaces
{
    public interface IRoutesRepository
    {
        Dictionary<Agency, List<Route>> GetAllRoutes();
        Route GetRouteById(string id);
        List<Route> GetRoutesList();
        IEnumerable<Route> GetRoutesServingStop(string stopId);
        Grid<string> GetSchedule(string route, DirectionType? dir, List<string> serviceIds, TimepointStrategy strat);
        bool HasAnyService(string route);
        Tuple<List<string>, bool> ServicesOn(LocalDate date);
        Dictionary<DirectionType?, RouteStats> GetRouteStats(LocalDate date, string route);
        IEnumerable<Tuple<Stop, bool>> GetRouteStops(string routeId, DirectionType? dir, TimepointStrategy strat);
        IEnumerable<Tuple<Stop, bool>> GetRouteStops(string routeId, DirectionType? dir);
    }
}