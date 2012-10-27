using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace Microsoft.Samples.Kinect.Avateering
{
    public class Task
    {
        public enum MediaType
        {
            Picture, Video, Text, Audio
        };

        public enum ProgressStatus
        {
            New, InWork, Done
        };

        public enum ManagerStatus
        {
            NotAssigned, Assigned, Finished
        };

        public enum PresentationType
        {
            IconBox, Window, PanelIcon
        };


        public MediaType mediaType;
        public ManagerStatus managerStatus;
        public ProgressStatus progressStatus;
        public PresentationType presentationType;
        public string mediaName;
        public Game game;
        public int operatorID;


        public PanelIcon panelIcon;

        public Task (Game _game, MediaType _mediaType, ManagerStatus _managerStatus, ProgressStatus _progressStatus, PresentationType _presentationType, String _mediaName, int _operatorID) 
            :base()
        {
            game = _game;
            mediaType = _mediaType;
            managerStatus = _managerStatus;
            progressStatus = _progressStatus;
            presentationType = _presentationType;
            mediaName = _mediaName;
            operatorID = _operatorID;
            
            AvateeringXNA.AllTasks.Add(this);
        }

        public void LoadContent(Game game)
        {

            switch (presentationType)
            {
                case PresentationType.PanelIcon:
                    {
                        panelIcon = new PanelIcon(game, mediaType, managerStatus, progressStatus, mediaName, 12);
                        AvateeringXNA.BossPanel.AllPanelIcons.Add(panelIcon);
                    }
                    break;
                case PresentationType.IconBox:
                    {

                    }
                    break;
                case PresentationType.Window:
                    {

                    }
                    break;
            }
            
            switch (mediaType)
            {
                case MediaType.Video:
                    {
                        
                    }
                    break;
                case MediaType.Audio:
                    {
                        
                    }
                    break;
                case MediaType.Picture:
                    {
                        
                    } 
                    break;
                case MediaType.Text:
                    {
                        
                    }
                    break;
            }
        }
    }
}
