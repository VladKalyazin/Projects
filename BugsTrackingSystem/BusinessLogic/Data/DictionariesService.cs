﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsignarDBEntities;
using AsignarServices.AzureStorage;
using BugsTrackingSystem.Models;
using System.Web.Mvc;

namespace AsignarServices.Data
{
    public partial class AsignarDataService : IDisposable
    {
        public IEnumerable<SelectListItem> GetProjectNames()
        {
            return _databaseModel.Projects.Select((p) => new SelectListItem
            {
                Value = p.ProjectID.ToString(),
                Text = p.ProjectName
            }).ToList();
        }

        public IEnumerable<SelectListItem> GetProjectNames(int userId)
        {
            var result = _databaseModel.Projects.Where(p => p.Users.Any(u => u.UserID == userId)).Select((p) => new SelectListItem
            {
                Value = p.ProjectID.ToString(),
                Text = p.ProjectName
            }).ToList();

            return result;
        }

        public IEnumerable<SelectListItem> GetProjectNames(int firstUserId, int secondUserId)
        {
            var result = _databaseModel.Projects.Where(p => p.Users.Any(u => u.UserID == firstUserId) && p.Users.Any(u => u.UserID == secondUserId)).Select((p) => new SelectListItem
            {
                Value = p.ProjectID.ToString(),
                Text = p.ProjectName
            }).ToList();

            return result;
        }

        public IEnumerable<SelectListItem> GetUserNames()
        {
            var result = _databaseModel.Users.Select((u) => new SelectListItem
            {
                Value = u.UserID.ToString(),
                Text = u.FirstName + " " + u.Surname
            }).ToList();

            //var firstItem = result.FirstOrDefault();
            //if (firstItem != null)
            //{
            //    firstItem.Selected = true;
            //}

            return result;
        }

        public IEnumerable<SelectListItem> GetUserNames(int projectId)
        {
            var result = _databaseModel.Users.Where(u => u.Projects.Any(p => p.ProjectID == projectId)).Select((u) => new SelectListItem
            {
                Value = u.UserID.ToString(),
                Text = u.FirstName + " " + u.Surname
            }).ToList();


            return result;
        }

        public IEnumerable<SelectListItem> GetPrioritiesNames()
        {
            return _databaseModel.DefectPriorities.Select((dp) => new SelectListItem
            {
                Value = dp.DefectPriorityID.ToString(),
                Text = dp.PriorityName
            }).ToList();
        }

        public IEnumerable<SelectListItem> GetStatusNames()
        {
            return _databaseModel.DefectStatuses.Select((s) => new SelectListItem
            {
                Value = s.DefectStatusID.ToString(),
                Text = s.StatusName
            }).ToList();
        }

        public IEnumerable<SelectListItem> GetRoleNames()
        {
            return _databaseModel.Roles.Select((r) => new SelectListItem
            {
                Value = r.RoleID.ToString(),
                Text = r.RoleName
            }).ToList();
        }
    }
}