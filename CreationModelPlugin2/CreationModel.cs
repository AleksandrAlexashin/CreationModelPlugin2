using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;
            //Level level1, level2;
            GetLevel(doc, out Level level1, out Level level2);
            CreatWall(doc, level1, level2, out List<Wall> walls);
            CreatDoor(doc, level1, walls);
            CreatWindows(doc, level1, walls);

            return Result.Succeeded;
        }

        private void CreatWindows(Document doc, Level level1,  List<Wall> walls)
        {
            var windowType = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(BuiltInCategory.OST_Windows)
            .OfType<FamilySymbol>()
            .Where(x => x.Name.Equals("0610 x 1830 мм"))
            .Where(x => x.FamilyName.Equals("Фиксированные"))
            .FirstOrDefault();
           

            Transaction tr = new Transaction(doc);
            for (int i = 1; i < 4; i++)

            {
                LocationCurve hostCurve = walls[i].Location as LocationCurve;
                XYZ p1 = hostCurve.Curve.GetEndPoint(0);
                XYZ p2 = hostCurve.Curve.GetEndPoint(1);
                XYZ point = (p1 + p2) / 2;
                tr.Start("Создание jryf");
                if (!windowType.IsActive)
                    windowType.Activate();
                var fm=doc.Create.NewFamilyInstance(point, windowType, walls[i], level1, StructuralType.NonStructural);
                if(fm is FamilyInstance)
                fm.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(UnitUtils.ConvertToInternalUnits(1000, UnitTypeId.Millimeters));
                tr.Commit();
            }
           
        }

        private void CreatDoor(Document doc, Level level1, List<Wall> walls)
        {

            
            var doorType = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(BuiltInCategory.OST_Doors)
            .OfType<FamilySymbol>()
            .Where(x => x.Name.Equals("0915 x 2134 мм"))
            .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
            .FirstOrDefault();
            LocationCurve hostCurve = walls[0].Location as LocationCurve;
            XYZ p1 = hostCurve.Curve.GetEndPoint(0);
            XYZ p2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (p1 + p2) / 2;
            Transaction tr = new Transaction(doc);
            tr.Start("Создание двери");
            if (!doorType.IsActive)
                doorType.Activate();
            doc.Create.NewFamilyInstance(point, doorType, walls[0], level1, StructuralType.NonStructural);
              tr.Commit();


        }

        private static void GetLevel(Document doc, out Level level1, out Level level2)
        {
            var listlevel = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .OfType<Level>()
                        .ToList();

            level1 = listlevel
                .Where(x => x.Name.Equals("Уровень 1"))
                .FirstOrDefault();
            level2 = listlevel
           .Where(x => x.Name.Equals("Уровень 2"))
           .FirstOrDefault();
        }

        private static void CreatWall(Document doc, Level level1, Level level2, out List<Wall> walls)
        {
            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            var points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            walls = new List<Wall>();
            

            Transaction tr = new Transaction(doc);
            tr.Start("Построение стен");

            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);

            }
            

            tr.Commit();
        }
    }
}
