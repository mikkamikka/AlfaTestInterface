using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Samples.Kinect.Avateering
{
    public class IconBoxVideo : IconBox
    {
        private string videoclipName { get; set; }
        Matrix view;
        Matrix projection;
        private VideoWindow vid_sample;
        public float time;

        
        public IconBoxVideo(Game game, string _videoclipName)
            : base(game)
        {
            game.Components.Add(this);
            videoclipName = _videoclipName;
            bb_size = new Vector3(4f, 3f, 1f);
            
        }
        protected override void LoadContent()
        {
            //Model = Game.Content.Load<Model>("videobox_2");
            LoadContent(Game.Content, "icon_videobox");

            //=============video

            vid_sample = new VideoWindow(Game);
            vid_sample.LoadContent(Game.Content, "plane2", videoclipName);

            //============video endof
            
            base.LoadContent();

        }

        public override void Draw(GameTime gameTime)
        {

            vid_sample.Update(gameTime);

            view = AvateeringXNA.view;
            projection = AvateeringXNA.projection;


            if (vid_sample.videoWindowIsOpen)
            {
                vid_sample.Draw(); 
                
            }

            // открываем окно с медиа, убираем иконку из списка иконок рабочего стола
            if (vid_sample.videoWindowIsOpen)
            {
                vid_sample.Draw(gameTime);                //рисуем окно с медиа
            }
            else
            {
                Draw();                                    //если окно с медиа закрыто - рисуем соответствующую ей иконку
            }

            // триггер включения иконки в список иконок рабочего стола
            if (!vid_sample.videoWindowIsOpen & vid_sample.prevoiusVideoWindowIsOpen)       // если состояние медиа окна меняется с открытого на закрытое
            {
                if (index < AvateeringXNA.AllIconBoxes.Count)
                {
                    AvateeringXNA.AllIconBoxes.Insert(index, this); // вставляем иконку в список в сохраненную позицию
                    //BoundingBox = parkBoundingBox;
                    Position.Y = 0f;                                // восстанавливаем позицию иконки, чтобы вернуть bounding box на рабочий стол
                }
                else                                                // если сохраненный индекс больше числа иконок 
                {
                    AvateeringXNA.AllIconBoxes.Add(this);           // вставляем иконку в конец списка
                    Position.Y = 0f;                                // восстанавливаем позицию иконки, чтобы вернуть bounding box на рабочий стол
                }
            }



            Collide(gameTime);

            time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            base.Draw(gameTime);
        }

        private void Collide(GameTime gameTime)
        {
            collideResults1 = BoundingBox.Contains(AvateeringXNA.CursorRight.boundingBox);

            if (!GestureAnyIsActive)
            {
                switch (collideResults1)
                {
                    case ContainmentType.Contains:
                        break;
                    case ContainmentType.Disjoint:
                        //current_hilite = new Vector3(0, 0, 0);
                        // keepCursorCoordinateZ = false;
                        break;
                    case ContainmentType.Intersects:
                        {
                            //keepCursorCoordinateZ = true;
                            //focusedObject = Video_box2;
                            if (AvateeringXNA.cursorVectorDirection.Z > 0.3f)
                            {
                                if ((previousCollideResult1 == ContainmentType.Disjoint) &&
                                    (vid_sample.player.State == MediaState.Playing))
                                {
                                    //GestureAnyIsActive = true; timer.Start(1000f);
                                    //vid_sample.BringToBack();
                                    //PressButtonEffectActivate();

                                }
                                else if ((previousCollideResult1 == ContainmentType.Disjoint) &&
                                         (vid_sample.player.State != MediaState.Playing))
                                {

                                    GestureAnyIsActive = true; timer.Start(1000f);
                                    //PressButtonEffectActivate();
                                    vid_sample.BringToFront();
                                    vid_sample.StartPlaying();
                                    HideIconBox();
                                }
                            }
                        }
                        break;
                        
                    default:
                        break;
                }
                previousCollideResult1 = collideResults1;
            }
            else
            {
                if (timer.IsActive) { timer.Update(gameTime); }
                else
                {
                    GestureAnyIsActive = false;
                }
            }
        }


    }
}


