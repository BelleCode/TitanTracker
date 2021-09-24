using Microsoft.AspNetCore.Mvc;
using TitanTracker.Data;
using System.Collections.Generic;
using System.Linq;
using TitanTracker.Models.ChartModels;
using TitanTracker.Models.Enums;
using System;

namespace TitanTracker.Controllers
{
    public class ChartsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly List<string> _backgroundColors;            // For Chart.js (no colour required for Chartist)

        public ChartsController(ApplicationDbContext context)
        {
            _context = context;
            _backgroundColors = new List<string>
            {
                "#C84DEB",
                "#EDBE58",
                "#53A8ED",
                "#7454DE",
                "#E0955D",
                "#59E0D4",
                "#4D7DEB",
                "#ED6D58",
                "#53ED89",
                "#CCCCCC"
            };
        }

        public JsonResult PriorityChart()
        {
            var result = new ChartJSModel();
            int count = 0;

            foreach (var priority in Enum.GetValues(typeof(BTTicketPriority)))
            {
                var priorityString = priority.ToString();

                result.Labels.Add(priorityString);

                var test = (BTTicketPriority)Enum.Parse(typeof(BTTicketPriority), priorityString);      // casting 

                result.Data.Add(_context.Tickets.Where(t => t.TicketPriority == test).Count());

                if (count < 10)
                {
                    result.BackgroundColor.Add(_backgroundColors[count]);
                }
                else
                {
                    result.BackgroundColor.Add(_backgroundColors[(count % 10)]);
                }
                count++;
            }

            return Json(result);
        }

        public JsonResult ProjectStatusChart()
        {
            var result = new ChartJSModel();
            int count = 0;

            foreach (var status in Enum.GetValues(typeof(BTProjectStatus)))
            {
                var statusString = status.ToString();

                result.Labels.Add(statusString);

                var test = (BTProjectStatus)Enum.Parse(typeof(BTProjectStatus), statusString);      // casting 

                result.Data.Add(_context.Projects.Where(t => t.ProjectStatus == test).Count());

                if (count < 10)
                {
                    result.BackgroundColor.Add(_backgroundColors[count]);
                }
                else
                {
                    result.BackgroundColor.Add(_backgroundColors[(count % 10)]);
                }
                count++;
            }

            return Json(result);
        }
    }
}