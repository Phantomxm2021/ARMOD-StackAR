using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace com.Phantoms.ARMODPackageTools.Core
{
    public class ProjectDataModel : ScriptableObject
    {
        public int CurrentOpenProjectId;
        public int CurrentContentViewId;
        public List<ProjectListElementModel> ProjectTreeElements = new List<ProjectListElementModel>();

        public void Init()
        {
            if (ProjectTreeElements.Count > 0) return;
            var tmp_ProjectTreeElements = new ProjectListElementModel("Root", -1, -1, string.Empty);
            ProjectTreeElements.Add(tmp_ProjectTreeElements);
        }


        public ProjectListElementModel GetOpeningProject()
        {
            if (CurrentOpenProjectId < 0
                || ProjectTreeElements == null
                || ProjectTreeElements.Count <= CurrentOpenProjectId) return null;

            return ProjectTreeElements[CurrentOpenProjectId + 1];
        }

        
        public Configures GetOpeningProjectConfigure()
        {
            Assert.IsNotNull(GetOpeningProject()?.Configures);
            return GetOpeningProject()?.Configures ? GetOpeningProject().Configures : null;
        }
    }
}