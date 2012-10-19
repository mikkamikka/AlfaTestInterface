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
    public class IconBoxPicture : IconBox
    {
        private string pictureName { get; set; }
        Matrix view;
        Matrix projection;
        private PictureWindow pic_sample;
        public float time;


        public IconBoxPicture(Game game, string _pictureName)
            : base(game)
        {
            game.Components.Add(this);
            pictureName = _pictureName;
            bb_size = new Vector3(4f, 3f, 1f);
        }
        protected override void LoadContent()
        {
            //Model = Game.Content.Load<Model>("videobox_2");
            LoadContent(Game.Content, "icon_picturebox");

            

            pic_sample = new PictureWindow(Game);
            pic_sample.LoadContent(Game.Content, "plane_picture", pictureName);


            

            base.LoadContent();

        }

        public override void Draw(GameTime gameTime)
        {

            pic_sample.Update(gameTime);

            view = AvateeringXNA.view;
            projection = AvateeringXNA.projection;

            
            
            // открываем окно с медиа, убираем иконку из списка иконок рабочего стола
            if (pic_sample.pictureWindowIsOpen)            //если окно с медиа открыто
            {
                pic_sample.Draw();                         //рисуем окно с медиа

            }
            else
            {
                Draw();                                    //если окно с медиа закрыто - рисуем соответствующую ей иконку
            }


            // триггер включения иконки в список иконок рабочего стола
            if (!pic_sample.pictureWindowIsOpen & pic_sample.previousPictureWindowIsOpen)         // если состояние медиа окна меняется с открытого на закрытое
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



           // if (pic_sample.pictureWindowIsOpen) current_hilite = new Vector3(0.3f, 0.3f, 0.3f);
           // else current_hilite = new Vector3(0.0f, 0.0f, 0.0f);

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
                        // Window.Title = "Contains";
                        break;
                    case ContainmentType.Disjoint:
                        // Window.Title = "Disjoint";
                        //current_hilite = new Vector3(0, 0, 0);
                        // keepCursorCoordinateZ = false;
                        break;
                    case ContainmentType.Intersects:
                        {
                            //Window.Title = "Intersects";
                            //keepCursorCoordinateZ = true;
                            //focusedObject = Video_box2;
                            if (AvateeringXNA.cursorVectorDirection.Z > 0.3f)
                            {
                                if ((previousCollideResult1 == ContainmentType.Disjoint) &&
                                    pic_sample.WindowIsOnFront)
                                {
                                    //GestureAnyIsActive = true; timer.Start(1000f);
                                    //pic_sample.BringToBack();
                                    //PressButtonEffectActivate();

                                }
                                else if ((previousCollideResult1 == ContainmentType.Disjoint) &&
                                    !pic_sample.WindowIsOnFront)
                                {

                                    GestureAnyIsActive = true; timer.Start(1000f);
                                    //PressButtonEffectActivate();
                                    
                                    pic_sample.StartShowing();
                                    pic_sample.BringToFront();
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


