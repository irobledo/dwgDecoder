using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

 
namespace fi.upm.es.dwgDecoder
{
 
    public class dwgDecoder
    {
 
        private static KeepStraightOverrule myOverrule;

        [CommandMethod("TestNacho")]
        public static void TestNacho()
        {
            Database db = Application.DocumentManager.MdiActiveDocument.Database;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            using (Transaction t = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction())
            {

                // Open the Block table for read
                BlockTable acBlkTbl = (BlockTable) t.GetObject(db.BlockTableId,OpenMode.ForRead);
 
                // Open the Block table record Model space for read
                BlockTableRecord acBlkTblRec = (BlockTableRecord) t.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],OpenMode.ForRead);
 
                // Step through the Block table record
                foreach (ObjectId acObjId in acBlkTblRec)
                {
                    ed.WriteMessage("\nDXF name: " + acObjId.ObjectClass.DxfName);
                    ed.WriteMessage("\nObjectID: " + acObjId.ToString());
                    ed.WriteMessage("\nHandle: " + acObjId.Handle.ToString());
                    ed.WriteMessage("\n");
                }
 
                LayerTable acLyrTbl = (LayerTable)t.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId acObjId in acLyrTbl)
                {
                    LayerTableRecord acLyrTblRec = (LayerTableRecord)t.GetObject(acObjId, OpenMode.ForRead);
                    ed.WriteMessage("Capa:" + acLyrTblRec.Name);
                }
            

            }
             
        }
        // Definimos el comando para el usuario. Cada vez que el usuario teclee
        // "KeepStraight" este método será invocado.
        // Al ser declarado static Autocad no instanciara una nueva clase cada
        // vez que llamemos al comando.

        [CommandMethod("KeepStraight")]
        public static void ImplementOverrule()
        { 
            // We only want to create our overrule instance once, 
            // so we check if it already exists before we create it
            // (i.e. this may be the 2nd time we've run the command)
             if (myOverrule  == null)
             {
                 // Instantiate our overrule class
                myOverrule = new KeepStraightOverrule();
                
                 // Register the overrule
                Overrule.AddOverrule(RXClass.GetClass(new AttributeReference().GetType()),myOverrule, false);
             }
            // Make sure overruling is turned on so our overrule works
            Overrule.Overruling = true; 
        }
    }
 
    // Clase que personaliza el comportamiento al transformar una entidad (mover, rotar...)
    public class KeepStraightOverrule : TransformOverrule
    {
        // La función estandar completa la ejecución del comano tal y como es.
        // Nosotros vamos a alterar ese comportamiento sobreescribiendo el metodo.
        public override void TransformBy(Entity entity,
                                         Matrix3d transform)
        {
            // Invocamos el metodo de transformacion estandar en la base.
            base.TransformBy(entity, transform);
            //La regla esta solo registrada para AttributeReference por lo que podemos
            // hacer el cast con seguridad.
            AttributeReference attRef  = (AttributeReference) entity;
            //Definimos la rotación a cero.
            attRef.Rotation = 0.0;
 
        }
    }
}