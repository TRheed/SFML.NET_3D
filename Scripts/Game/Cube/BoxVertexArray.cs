using SFML.Window;
using SFML.System;
using SFML.Audio;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using static SFML_NET_3D.Data;
using static SFML_NET_3D.Constants;
using static SFML_NET_3D.Utility;

namespace SFML_NET_3D
{
    public class BoxVertexArray
    {
        public List<BoxVertex> ToList { get => list; }
        public PrimitiveType Type { get; set; }
        public Color FillColor
        {
            get => fillCol; set
            {
                fillCol = value;
                SetPlaneFillColor();
            }
        }

        List<BoxVertex> list;
        Side[,] vertexSides;
        Color[] planeColors;
        Color fillCol;

        public BoxVertexArray(Color fillColor, PrimitiveType type = PrimitiveType.LineStrip)
        {
            list = new List<BoxVertex>();
            this.Type = type;
            this.FillColor = fillColor;
            vertexSides = new Side[6, 4]
            {
                {Side.LTF,Side.RTF,Side.RBF,Side.LBF},
                {Side.LTB,Side.RTB,Side.RBB,Side.LBB},
                {Side.LTB,Side.RTB,Side.RTF,Side.LTF},
                {Side.RTB,Side.RBB,Side.RBF,Side.RTF},
                {Side.RBB,Side.LBB,Side.LBF,Side.RBF},
                {Side.LBB,Side.LTB,Side.LTF,Side.LBF}
            };
            SetPlaneFillColor();
        }

        private void SetPlaneFillColor()
        {
            planeColors = new Color[6]
            {
                new Color(0,0,0) + FillColor,
                new Color(20,20,20) + FillColor,
                new Color(40,40,40) + FillColor,
                new Color(60,60,60) + FillColor,
                new Color(80,80,80) + FillColor,
                new Color(100,100,100) + FillColor
            };
        }

        public void Append(BoxVertex vertex)
        {
            list.Add(vertex);
        }
        
        public void Display()
        {
            //Add Planes Into Plane List
            List<PlaneVertexArray> planes = 
            new List<PlaneVertexArray>();
            for (int i = 0; i < 6; i++)
            {
                PlaneVertexArray plane;
                SetPlaneData(out plane, i);
                planes.Add(plane);
            }

            //Sort Plane List By Z-Order
            planes = SortByZOrder(planes, 0, planes.Count - 1);

            //Display Planes One By One;
            for (int i = 0; i < 6; i++)
                planes[i].Display();
            planes.Clear();
        }

        private void SetPlaneData(out PlaneVertexArray plane, int index)
        {
            plane = new PlaneVertexArray(Type, planeColors[index]);
            for (int j = 0; j < 4; j++)
            {
                BoxVertex v3D = GetVertexFromSide(vertexSides[index, j]);
                plane.Append(v3D);
            }
        }

        private List<PlaneVertexArray> SortByZOrder(List<PlaneVertexArray> list, int start, int end)
        {
            int i;
            if (start < end)
            {
                i = Partition(list, start, end);

                SortByZOrder(list, start, i - 1);
                SortByZOrder(list, i + 1, end);
            }

            return list;
        }

        private int Partition(List<PlaneVertexArray> list, int start, int end)
        {
            PlaneVertexArray temp;
            PlaneVertexArray p = list[end];
            int i = start - 1;

            for (int j = start; j <= end - 1; j++)
            {
                if (list[j].GetDepth() <= p.GetDepth())
                {
                    i++;
                    temp = list[i];
                    list[i] = list[j];
                    list[j] = temp;
                }
            }

            temp = list[i + 1];
            list[i + 1] = list[end];
            list[end] = temp;
            return i + 1;
        }

        private BoxVertex GetVertexFromSide(Side side)
        {
            foreach (var vertex in list)
                if (vertex.Side == side) return vertex;

            return list[0];
        }
    }
}