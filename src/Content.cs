using Foster.Framework;
using FosterPlatformer.Assets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace FosterPlatformer
{
    public class Content
    {
        public static SpriteFont Font;

        private struct RoomInfo
        {
            public Bitmap Image;
            public Point2 Cell;
        }

        private struct SpriteInfo
        {
            public string Name;
            public Aseprite Aseprite;
            public int PackIndex;
        }

        private static string Root = "";
        private static List<Sprite> Sprites = new List<Sprite>();
        private static List<Tileset> Tilesets = new List<Tileset>();
        private static List<Subtexture> Subtextures = new List<Subtexture>();
        private static List<RoomInfo> Rooms = new List<RoomInfo>();
        private static Atlas SpriteAtlas;

        /// <summary>
        ///
        /// </summary>
        public static string Path()
        {
            if (Root.Length <= 0) {
                string up = "";

                do {
                    Root = System.IO.Path.GetFullPath(String.Format("{0}/{1}content/", AppDomain.CurrentDomain.BaseDirectory, up));
                    up = up + "../";
                } while (!Directory.Exists(Root) && up.Length < 30);

                if (!Directory.Exists(Root)) {
                    Log.Error("Unable to find content directory.");
                }

                Log.Message(String.Format("Content Path: {0}", Root));
            }

            return Root;
        }

        /// <summary>
        ///
        /// </summary>
        public static void Load()
        {
            Packer packer = new Packer();
            packer.Padding = 0;

            // Load the main font.
            Font = new SpriteFont(Path() + "fonts/dogica.ttf", 8, "abcdefghijklmnopqrstuvwxyz"); // @TODO Revisit charset.
            Font.LineGap = 4;

            int packIndex = 0;

            #region Load sprites

            List<SpriteInfo> spriteInfo = new List<SpriteInfo>();

            // Get all sprites.
            string spritePath = Path() + "sprites/";
            foreach (string it in Directory.EnumerateFiles(spritePath)) {
                if (!it.EndsWith(".ase"))
                    continue;

                SpriteInfo info = new SpriteInfo();
                info.Aseprite = new Aseprite(it);
                info.Name = it.Substring(0, it.Length - 4);
                info.PackIndex = packIndex;

                foreach (var frame in info.Aseprite.Frames) {
                    packer.AddBitmap(packIndex.ToString(), frame.Bitmap);
                    packIndex++;
                }

                spriteInfo.Add(info);
            }

            #endregion

            #region Load tilesets

            List<SpriteInfo> tilesetInfo = new List<SpriteInfo>();

            // Get all tilesets.
            string tilesetPath = Path() + "tilesets/";
            foreach (string it in Directory.EnumerateFiles(tilesetPath)) {
                if (!it.EndsWith(".ase"))
                    continue;

                SpriteInfo info = new SpriteInfo();
                info.Aseprite = new Aseprite(it);
                info.Name = it.Substring(0, it.Length - 4);
                info.PackIndex = packIndex;

                var frame = info.Aseprite.Frames[0];
                var columns = frame.Bitmap.Width / Game.TileWidth;
                var rows = frame.Bitmap.Height / Game.TileHeight;

                for (int x = 0; x < columns; x++) {
                    for (int y = 0; y < rows; y++) {
                        var subrect = new RectInt(x * Game.TileWidth, y * Game.TileHeight, Game.TileWidth, Game.TileHeight);
                        var subimage = frame.Bitmap.GetSubBitmap(subrect);
                        packer.AddBitmap(packIndex.ToString(), subimage);
                        packIndex++;
                    }
                }
            }

            #endregion

            // Build the atlas.
            packer.Pack();
            SpriteAtlas = new Atlas(packer);

            #region Add sprites

            foreach (SpriteInfo info in spriteInfo) {
                Sprite sprite = new Sprite();
                sprite.Name = info.Name;
                sprite.Origin = Vector2.Zero;
                sprite.Animations = new List<Sprite.Animation>();

                if (info.Aseprite.Slices.Count > 0 && info.Aseprite.Slices[0].Pivot.HasValue) {
                    sprite.Origin = new Vector2(
                        info.Aseprite.Slices[0].Pivot.Value.X,
                        info.Aseprite.Slices[0].Pivot.Value.Y
                    );
                }

                foreach (var tag in info.Aseprite.Tags) {
                    Sprite.Animation anim = new Sprite.Animation();
                    anim.Name = tag.Name;
                    anim.Frames = new List<Sprite.Frame>();

                    for (int i = tag.From; i <= tag.To; i++) {
                        Sprite.Frame frame = new Sprite.Frame();
                        frame.Duration = info.Aseprite.Frames[i].Duration / 1000.0f;
                        frame.Image = SpriteAtlas.Subtextures[(info.PackIndex + i).ToString()];
                        anim.Frames.Add(frame);
                    }

                    sprite.Animations.Add(anim);
                }

                Sprites.Add(sprite);
            }

            #endregion

            #region Add Tilesets

            foreach (SpriteInfo info in tilesetInfo) {
                var frame = info.Aseprite.Frames[0];

                Tileset tileset = new Tileset();
                tileset.Name = info.Name;
                tileset.Columns = frame.Bitmap.Width / Game.TileWidth;
                tileset.Rows = frame.Bitmap.Height / Game.TileHeight;

                for (int x = 0, i = info.PackIndex; x < tileset.Columns; x++) {
                    for (int y = 0; y < tileset.Rows; y++) {
                        tileset.Tiles[x + y * tileset.Columns] = SpriteAtlas.Subtextures[i.ToString()];
                        i++;
                    }
                }
            }

            #endregion

            // Load the rooms.
            string mapPath = Path() + "map/";
            foreach (string it in Directory.EnumerateFiles(mapPath)) {
                if (!it.EndsWith(".png"))
                    continue;

                var name = System.IO.Path.GetFileNameWithoutExtension(it);
                var point = name.Split('x');
                if (point.Length != 2)
                    continue;

                RoomInfo info;
                info.Cell.X = int.Parse(point[0]);
                info.Cell.Y = int.Parse(point[1]);
                info.Image = new Bitmap(it);

                Debug.Assert(info.Image.Width == Game.Columns, "Room is incorrect width!");
                Debug.Assert(info.Image.Height == Game.Rows, "Room is incorrect height!");

                Rooms.Add(info);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public static void Unload()
        {
            //
        }

        /// <summary>
        ///
        /// </summary>
        public static Atlas Atlas()
        {
            return SpriteAtlas;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public static Sprite FindSprite(string name)
        {
            foreach (Sprite it in Sprites)
                if (it.Name == name)
                    return it;

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        public static Tileset FindTileset(string name)
        {
            foreach (Tileset it in Tilesets)
                if (it.Name == name)
                    return it;

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="cell"></param>
        public static Bitmap FindRoom(Point2 cell)
        {
            foreach (RoomInfo it in Rooms)
                if (it.Cell == cell)
                    return it.Image;

            return null;
        }
    }
}
