﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsignarDBEntities;
using AsignarServices.AzureStorage;
using BugsTrackingSystem.Models;

namespace AsignarServices.Data
{
    public partial class AsignarDataService : IDisposable
    {
        public IEnumerable<DefectViewModel> GetUserDefects(int userId)
        {
            return (from defect in _databaseModel.Defects
                    where defect.AssigneeUserID == userId && 
                    _databaseModel.Users.FirstOrDefault(u => u.UserID == userId).Projects.Any(p => p.ProjectID == defect.ProjectID)
                    select new DefectViewModel
                    {
                        DefectId = defect.DefectID,
                        Subject = defect.Subject,
                        AssigneeUserName = defect.User.FirstName + " " + defect.User.Surname,
                        Status = defect.DefectStatus.StatusName,
                        PriorityPhoto = defect.DefectPriority.PhotoLink,
                        AssigneeUserPhoto = defect.User.PhotoLink,
                        CreationDate = defect.CreationDate,
                        ModificationDate = defect.ModificationDate
                    }).ToList();
        }

        public int GetCountUserDefects(int userId) =>
            _databaseModel.Defects.Where((d) => d.AssigneeUserID == userId &&
            _databaseModel.Users.FirstOrDefault(u => u.UserID == userId).Projects.Any(p => p.ProjectID == d.ProjectID)).Count();

        public IEnumerable<DefectViewModel> GetUserSetOfDefects(int userId, int countOfSet, int page)
        {
            try
            {
                return (from defect in _databaseModel.Defects.OrderBy((p) => p.CreationDate)
                        where defect.AssigneeUserID == userId &&
                        _databaseModel.Users.FirstOrDefault(u => u.UserID == userId).Projects.Any(p => p.ProjectID == defect.ProjectID)
                        select new DefectViewModel
                        {
                            DefectId = defect.DefectID,
                            Subject = defect.Subject,
                            AssigneeUserName = defect.User.FirstName + " " + defect.User.Surname,
                            Status = defect.DefectStatus.StatusName,
                            PriorityPhoto = defect.DefectPriority.PhotoLink,
                            AssigneeUserPhoto = defect.User.PhotoLink,
                            CreationDate = defect.CreationDate,
                            ModificationDate = defect.ModificationDate
                        }).Skip(page * countOfSet).Take(countOfSet).ToList();
            }
            catch (Exception)
            {
                //TODO exceptions
            }

            return null;
        }

        public DefectExtendedViewModel GetDefectExtendedInfo(int defectId)
        {
            try
            {
                var result = (from defect in _databaseModel.Defects
                              where defect.DefectID == defectId
                              select new DefectExtendedViewModel
                              {
                                  DefectId = defect.DefectID,
                                  Subject = defect.Subject,
                                  Prefix = defect.Project.Prefix + "-" + defect.DefectID,
                                  ProjectId = defect.ProjectID,
                                  ProjectName = defect.Project.ProjectName,
                                  AssigneeUserName = defect.User.FirstName + " " + defect.User.Surname,
                                  StatusId = defect.DefectStatusID,
                                  StatusName = defect.DefectStatus.StatusName,
                                  UserId = defect.AssigneeUserID,
                                  PriorityId = defect.DefectPriorityID,
                                  PriorityName = defect.DefectPriority.PriorityName,
                                  PriorityPhoto = defect.DefectPriority.PhotoLink,
                                  AssigneeUserPhoto = defect.User.PhotoLink,
                                  AssigneeUserEmail = defect.User.Email,
                                  CreationDate = defect.CreationDate,
                                  ModificationDate = defect.ModificationDate,
                                  Description = defect.Description,
                                  Attachments = defect.DefectAttachments.Select(a => new AttachmentViewModel()
                                  {
                                      AttachmentId = a.AttachmentID,
                                      Name = a.Name,
                                      Link = a.Link
                                  }).ToList()
                              }).SingleOrDefault();

                result.StatusesList = GetStatusNames();

                TableStorageHelper tableHelper = new TableStorageHelper();
                result.Comments = tableHelper.GetDefectComments(defectId, _databaseModel);

                return result;
            }
            catch (Exception)
            {
                //TODO exceptions
            }

            return null;
        }

        public void AddDefect(DefectAddEditViewModel defect)
        {
            try
            {
                var newDefect = new Defect
                {
                    Subject = defect.Name,
                    ProjectID = defect.ProjectId,
                    AssigneeUserID = defect.UserId,
                    DefectPriorityID = defect.PriorityId,
                    DefectStatusID = defect.StatusId,
                    Description = defect.Description,
                    CreationDate = DateTime.UtcNow
                };
                _databaseModel.Defects.Add(newDefect);
                _databaseModel.SaveChanges();

                defect.DefectId = newDefect.DefectID;
            }
            catch (Exception)
            {
                throw new Exception("Can not create a task with this parameters.");
            }

        }

        public void EditDefect(DefectAddEditViewModel defect)
        {
            try
            {
                var editEntity = _databaseModel.Defects.First((d) => d.DefectID == defect.DefectId);
                editEntity.Subject = defect.Name;
                editEntity.ProjectID = defect.ProjectId;
                editEntity.AssigneeUserID = defect.UserId;
                editEntity.DefectPriorityID = defect.PriorityId;
                editEntity.DefectStatusID = defect.StatusId;
                editEntity.Description = defect.Description;
                editEntity.ModificationDate = DateTime.UtcNow;

                _databaseModel.Defects.Attach(editEntity);

                var entry = _databaseModel.Entry(editEntity);
                entry.Property((d) => d.Subject).IsModified = true;
                entry.Property((d) => d.ProjectID).IsModified = true;
                entry.Property((d) => d.AssigneeUserID).IsModified = true;
                entry.Property((d) => d.DefectPriorityID).IsModified = true;
                entry.Property((d) => d.DefectStatusID).IsModified = true;
                entry.Property((d) => d.Description).IsModified = true;
                entry.Property((d) => d.ModificationDate).IsModified = true;

                _databaseModel.SaveChanges();
            }
            catch (Exception)
            {
                throw new Exception("Can not edit a task with this parameters.");
            }
        }

        public void AssignDefect(int defectId, int userId)
        {
            try
            {
                var editEntity = _databaseModel.Defects.First((d) => d.DefectID == defectId);
                if (!_databaseModel.Users.First(u => u.UserID == userId).Projects.
                    Any(p => p.ProjectID == editEntity.ProjectID))
                {
                    throw new MethodAccessException("Firstly you have to assign to the project. Then you get permissions to reassign task.");
                }

                editEntity.AssigneeUserID = userId;
                editEntity.ModificationDate = DateTime.UtcNow;

                _databaseModel.Defects.Attach(editEntity);
                var entry = _databaseModel.Entry(editEntity);
                entry.Property((d) => d.AssigneeUserID).IsModified = true;
                entry.Property((d) => d.ModificationDate).IsModified = true;

                _databaseModel.SaveChanges();
            }
            catch (MethodAccessException e)
            {
                throw new Exception(e.Message);
            }
            catch
            {
                throw new Exception("Can not reassign the task.");
            }
        }

        public void DeleteDefect(int defectId)
        {
            try
            {
                _databaseModel.Defects.Remove(_databaseModel.Defects.First(d => d.DefectID == defectId));

                _databaseModel.SaveChanges();
            }
            catch
            {
                throw new Exception("Defect can not be deleted.");
            }

            try
            {
                var commentHelper = new AzureStorage.TableStorageHelper();
                commentHelper.DeleteComments(defectId);
            }
            catch
            {

            }
        }

        public void AddAttachment(int defectId, string name, byte[] data)
        {
            try
            {
                var attachment = new DefectAttachment
                {
                    DefectID = defectId,
                    Name = name
                };

                _databaseModel.DefectAttachments.Add(attachment);

                _databaseModel.SaveChanges();

                var blobHelper = new BlobStorageHelper();
                blobHelper.UploadAttachment(defectId, attachment.AttachmentID, data, name);
                string link = blobHelper.GetAttachmentUrl(defectId, attachment.AttachmentID);

                attachment.Link = link;

                _databaseModel.DefectAttachments.Attach(attachment);

                var entry = _databaseModel.Entry(attachment);
                entry.Property(e => e.Link).IsModified = true;

                _databaseModel.SaveChanges();
            }
            catch
            {

            }

        }

        public void DeleteAttachment(int attachmentId)
        {
            try
            {
                _databaseModel.DefectAttachments.Remove(_databaseModel.DefectAttachments.First(da => da.AttachmentID == attachmentId));

                _databaseModel.SaveChanges();

                var blobHelper = new BlobStorageHelper();
                blobHelper.DeleteAttachment(attachmentId);
            }
            catch
            {

            }
        }

        public IEnumerable<AttachmentViewModel> GetAttachments(int defectId)
        {
            try
            {
                return _databaseModel.DefectAttachments.Select(a => new AttachmentViewModel()
                {
                    AttachmentId = a.AttachmentID,
                    Name = a.Name,
                    Link = a.Link
                }).ToList();
            }
            catch
            {

            }

            return null;
        }

    }
}