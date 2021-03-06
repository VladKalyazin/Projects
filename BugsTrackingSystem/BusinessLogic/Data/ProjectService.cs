﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AsignarDBEntities;
using AsignarServices.AzureStorage;
using BugsTrackingSystem.Models;

namespace AsignarServices.Data
{
    public static class LINQExtension
    {
        public static IQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string orderByProperty,
                  bool desc)
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                          source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }
    }

    public enum DefectSortProperty
    {
        Title,
        Status,
        Date,
        Users,
        Priority
    }

    public enum SortOrder
    {
        Ascending,
        Descending
    }

    public partial class AsignarDataService : IDisposable
    {

        public IEnumerable<ProjectViewModel> GetAllProjects()
        {
            try
            {
                return from project in _databaseModel.Projects
                        select new ProjectViewModel
                        {
                            ProjectId = project.ProjectID,
                            Name = project.ProjectName,
                            Prefix = project.Prefix,
                            UsersCount = project.Users.Count,
                            DefectsCount = project.Defects.Count
                        };
            }
            catch
            {
                //TODO exceptions
            }

            return null;
        }

        public bool IsUserInProject(int userId, int projectId)
        {
            return _databaseModel.Projects.First(p => p.ProjectID == projectId).
                Users.Any(u => u.UserID == userId);
        }

        public int GetUserProjectsCount(int userId) =>
            _databaseModel.Projects.Where(p => p.Users.Any((u) => u.UserID == userId)).Count();

        public IEnumerable<ProjectViewModel> GetUserProjects(int userId)
        {
            try
            {
                return from project in _databaseModel.Projects
                       where project.Users.Any((u) => u.UserID == userId)
                       select new ProjectViewModel
                       {
                           ProjectId = project.ProjectID,
                           Name = project.ProjectName,
                           Prefix = project.Prefix,
                           UsersCount = project.Users.Count,
                           DefectsCount = project.Defects.Count
                       };
            }
            catch
            {
                //TODO exceptions
            }

            return null;
        }

        public void RemoveUserFromProject(int userId, int projectId)
        {
            try
            {
                _databaseModel.Projects.First(p => p.ProjectID == projectId).
                    Users.Remove(_databaseModel.Users.First(u => u.UserID == userId));

                _databaseModel.SaveChanges();
            }
            catch
            {

            }
        }

        public IEnumerable<ProjectViewModel> GetUserProjects(int userId, int countOfSet, int page)
        {
            try
            {
                return _databaseModel.Projects.Where(project => project.Users.Any((u) => u.UserID == userId)).OrderBy(p => p.ProjectName).       
                       Skip(page * countOfSet).Take(countOfSet).
                       Select(project => new ProjectViewModel
                       {
                           ProjectId = project.ProjectID,
                           Name = project.ProjectName,
                           Prefix = project.Prefix,
                           UsersCount = project.Users.Count,
                           DefectsCount = project.Defects.Count
                       });
            }
            catch
            {
                //TODO exceptions
            }

            return null;
        }

        public void EditProject(int projectId, string name)
        {
            try
            {
                _databaseModel.Projects.First((p) => p.ProjectID == projectId).ProjectName = name;
                _databaseModel.SaveChanges();
            }
            catch
            {

            }
        }

        public int GetCountOfProjects() =>
            _databaseModel.Projects.Count();

        public IEnumerable<ProjectViewModel> GetSetOfProjects(int countOfSet, int page)
        {
            try
            {
                return from project in _databaseModel.Projects.OrderBy((p) => p.ProjectName).Skip(page * countOfSet).Take(countOfSet)
                        select new ProjectViewModel
                        {
                            ProjectId = project.ProjectID,
                            Name = project.ProjectName,
                            Prefix = project.Prefix,
                            UsersCount = project.Users.Count,
                            DefectsCount = project.Defects.Count
                        };
            }
            catch
            {
                //TODO exceptions
            }

            return null;
        }

        public void AddProject(ProjectViewModel projectModel)
        {
            var newProject = new AsignarDBEntities.Project
            {
                ProjectName = projectModel.Name,
                Prefix = projectModel.Prefix.ToUpper(),
                CreationDate = DateTime.UtcNow
            };

            try
            {
                _databaseModel.Projects.Add(newProject);
                _databaseModel.SaveChanges();

                projectModel.ProjectId = newProject.ProjectID;
            }
            catch
            {
                //TODO exceptions
            }
        }

        public void DeleteProject(int projectId)
        {
            try
            {
                var project = _databaseModel.Projects.First(p => p.ProjectID == projectId);
                if (project.Users.Count > 0 || project.Defects.Count > 0)
                {
                    throw new MethodAccessException($"This project contains {project.Users.Count} users and {project.Defects.Count} tasks. Before deleting you have to clear this project");
                }
                _databaseModel.Projects.Remove(project);

                _databaseModel.SaveChanges();
            }
            catch (MethodAccessException e)
            {
                throw new Exception(e.Message);
            }
            catch
            {
                throw new Exception("Can not delete a project.");
            }
        }

        public ProjectExtendedViewModel GetFullProjectInfo(int projectId, DefectSortProperty sortProp = DefectSortProperty.Title, SortOrder sortOrder = SortOrder.Ascending)
        {
            try
            {
                string sortPropName = sortProp == DefectSortProperty.Title ? "Subject" :
                                        sortProp == DefectSortProperty.Status ? "DefectStatusID" :
                                        sortProp == DefectSortProperty.Date ? "CreationDate" :
                                        sortProp == DefectSortProperty.Priority ? "DefectPriorityID" :
                                        "AssigneeUserID";

                var defects = _databaseModel.Defects.Where((d) => d.ProjectID == projectId).
                            OrderBy(sortPropName, sortOrder == SortOrder.Descending ? true : false).
                            Select((defect) => new DefectViewModel
                            {
                                DefectId = defect.DefectID,
                                Subject = defect.Subject,
                                Prefix = defect.Project.Prefix + "-" + defect.DefectID,
                                AssigneeUserName = defect.User.FirstName + " " + defect.User.Surname,
                                Status = defect.DefectStatus.StatusName,
                                PriorityPhoto = defect.DefectPriority.PhotoLink,
                                AssigneeUserPhoto = defect.User.PhotoLink,
                                CreationDate = defect.CreationDate,
                                ModificationDate = defect.ModificationDate
                            }).ToList();

                var result = (from project in _databaseModel.Projects
                              where project.ProjectID == projectId
                              select new ProjectExtendedViewModel
                              {
                                  ProjectId = project.ProjectID,
                                  Name = project.ProjectName,
                                  Prefix = project.Prefix,
                                  UsersCount = project.Users.Count,
                                  DefectsCount = project.Defects.Count,
                                  Users = (from user in project.Users
                                           select new UserSimpleViewModel
                                           {
                                               UserId = user.UserID,
                                               FirstName = user.FirstName,
                                               Surname = user.Surname,
                                               Email = user.Email,
                                               DefectsCount = user.Defects.Count,
                                               ProjectsCount = user.Projects.Count,
                                               UserPhoto = user.PhotoLink
                                           }).ToList()
                              }).FirstOrDefault();

                result.Defects = defects;

                return result;
            }
            catch
            {
                //TODO exceptions
            }

            return null;
        }

        public ProjectExtendedViewModel GetFullProjectInfo(int projectId, int countOfSet, int page,
                    DefectSortProperty sortProp = DefectSortProperty.Title, 
                    SortOrder sortOrder = SortOrder.Ascending)
        {
            try
            {
                string sortPropName = sortProp == DefectSortProperty.Title ? "Subject" :
                                        sortProp == DefectSortProperty.Status ? "DefectStatusID" :
                                        sortProp == DefectSortProperty.Date ? "CreationDate" :
                                        "AssigneeUserID";

                var defects = _databaseModel.Defects.Where((d) => d.ProjectID == projectId).
                            OrderBy(sortPropName, sortOrder == SortOrder.Descending ? true : false).
                            Skip(page * countOfSet).Take(countOfSet).
                            Select((defect) => new DefectViewModel
                            {
                                DefectId = defect.DefectID,
                                Subject = defect.Subject,
                                Prefix = defect.Project.Prefix + "-" + defect.DefectID,
                                AssigneeUserName = defect.User.FirstName + " " + defect.User.Surname,
                                Status = defect.DefectStatus.StatusName,
                                PriorityPhoto = defect.DefectPriority.PhotoLink,
                                AssigneeUserPhoto = defect.User.PhotoLink,
                                CreationDate = defect.CreationDate,
                                ModificationDate = defect.ModificationDate
                            }).ToList();

                var result = (from project in _databaseModel.Projects
                              where project.ProjectID == projectId
                              select new ProjectExtendedViewModel
                              {
                                  ProjectId = project.ProjectID,
                                  Name = project.ProjectName,
                                  Prefix = project.Prefix,
                                  UsersCount = project.Users.Count,
                                  DefectsCount = project.Defects.Count,
                                  Users = (from user in project.Users
                                           select new UserSimpleViewModel
                                           {
                                               UserId = user.UserID,
                                               FirstName = user.FirstName,
                                               Surname = user.Surname,
                                               Email = user.Email,
                                               DefectsCount = user.Defects.Count,
                                               ProjectsCount = user.Projects.Count,
                                               UserPhoto = user.PhotoLink
                                           }).ToList()
                              }).FirstOrDefault();

                result.Defects = defects;

                return result;
            }
            catch
            {
                //TODO exceptions
            }

            return null;
        }

        public void AddUsersToProject(int projectId, IEnumerable<int> users)
        {
            try
            {
                using (var dbContextTransaction = _databaseModel.Database.BeginTransaction())
                {
                    foreach (var userId in users)
                    {
                        _databaseModel.Projects.Single(p => p.ProjectID == projectId).Users.
                            Add(_databaseModel.Users.First(u => u.UserID == userId));
                    }

                    _databaseModel.SaveChanges();

                    dbContextTransaction.Commit();
                }
            }
            catch
            {

            }
        }

    }
}
