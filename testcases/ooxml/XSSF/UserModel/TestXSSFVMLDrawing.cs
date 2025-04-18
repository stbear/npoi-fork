/* ====================================================================
   Licensed to the Apache Software Foundation (ASF) under one or more
   contributor license agreements.  See the NOTICE file distributed with
   this work for Additional information regarding copyright ownership.
   The ASF licenses this file to You under the Apache License, Version 2.0
   (the "License"); you may not use this file except in compliance with
   the License.  You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
==================================================================== */
using NPOI.OpenXmlFormats.Vml;
using NPOI.OpenXmlFormats.Vml.Office;
using NPOI.OpenXmlFormats.Vml.Spreadsheet;
using NPOI.XSSF.UserModel;
using NUnit.Framework;using NUnit.Framework.Legacy;
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

namespace TestCases.XSSF.UserModel
{

    /**
     * @author Yegor Kozlov
     */
    [TestFixture]
    public class TestXSSFVMLDrawing
    {
        [Test]
        public void TestNew()
        {
            XSSFVMLDrawing vml = new XSSFVMLDrawing();
            ArrayList items = vml.GetItems();
            ClassicAssert.AreEqual(2, items.Count);
            ClassicAssert.IsTrue(items[0] is CT_ShapeLayout);
            CT_ShapeLayout layout = (CT_ShapeLayout)items[0];
            ClassicAssert.AreEqual(ST_Ext.edit, layout.ext);
            ClassicAssert.AreEqual(ST_Ext.edit, layout.idmap.ext);
            ClassicAssert.AreEqual("1", layout.idmap.data);

            ClassicAssert.IsTrue(items[1] is CT_Shapetype);
            CT_Shapetype type = (CT_Shapetype)items[1];
            ClassicAssert.AreEqual("21600,21600", type.coordsize);
            ClassicAssert.AreEqual(202.0f, type.spt, 0);
            ClassicAssert.AreEqual("m,l,21600r21600,l21600,xe", type.path2);
            ClassicAssert.AreEqual("_x0000_t202", type.id);
            ClassicAssert.AreEqual(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t, type.path.gradientshapeok);
            ClassicAssert.AreEqual(ST_ConnectType.rect, type.path.connecttype);

            CT_Shape shape = vml.newCommentShape();
            ClassicAssert.AreEqual(3, items.Count);
            ClassicAssert.AreSame(items[2], shape);
            ClassicAssert.AreEqual("#_x0000_t202", shape.type);
            ClassicAssert.AreEqual("position:absolute; visibility:hidden", shape.style);
            ClassicAssert.AreEqual("#ffffe1", shape.fillcolor);
            ClassicAssert.AreEqual(ST_InsetMode.auto, shape.insetmode);
            ClassicAssert.AreEqual("#ffffe1", shape.fill.color);
            CT_Shadow shadow = shape.shadow;
            ClassicAssert.AreEqual(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t, shadow.on);
            ClassicAssert.AreEqual("black", shadow.color);
            ClassicAssert.AreEqual(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t, shadow.obscured);
            ClassicAssert.AreEqual(ST_ConnectType.none, shape.path.connecttype);
            ClassicAssert.AreEqual("mso-direction-alt:auto", shape.textbox.style);
            CT_ClientData cldata = shape.GetClientDataArray(0);
            ClassicAssert.AreEqual(ST_ObjectType.Note, cldata.ObjectType);
            ClassicAssert.AreEqual(1, cldata.SizeOfMoveWithCellsArray());
            ClassicAssert.AreEqual(1, cldata.SizeOfSizeWithCellsArray());
            ClassicAssert.AreEqual("1, 15, 0, 2, 3, 15, 3, 16", cldata.anchor);
            ClassicAssert.AreEqual(ST_TrueFalseBlank.@false, cldata.autoFill);
            ClassicAssert.AreEqual(0, cldata.GetRowArray(0));
            ClassicAssert.AreEqual(0, cldata.GetColumnArray(0));

            //each of the properties of CT_ClientData should occurs 0 or 1 times, and CT_ClientData has multiple properties.
            //ClassicAssert.AreEqual("[]", cldata.GetVisibleList().ToString());
            ClassicAssert.AreEqual(ST_TrueFalseBlank.NONE, cldata.visible);
            cldata.visible = (ST_TrueFalseBlank) Enum.Parse(typeof(ST_TrueFalseBlank), "true");
            ClassicAssert.AreEqual(ST_TrueFalseBlank.@true, cldata.visible);
            //serialize and read again
            MemoryStream out1 = new MemoryStream();
            vml.Write(out1);

            XSSFVMLDrawing vml2 = new XSSFVMLDrawing();
            vml2.Read(new MemoryStream(out1.ToArray()));
            ArrayList items2 = vml2.GetItems();
            ClassicAssert.AreEqual(3, items2.Count);
            ClassicAssert.IsTrue(items2[0] is CT_ShapeLayout);
            ClassicAssert.IsTrue(items2[1] is CT_Shapetype);
            ClassicAssert.IsTrue(items2[2] is CT_Shape);
        }
        [Test]
        public void TestFindCommentShape()
        {

            XSSFVMLDrawing vml = new XSSFVMLDrawing();
            vml.Read(POIDataSamples.GetSpreadSheetInstance().OpenResourceAsStream("vmlDrawing1.vml"));

            CT_Shape sh_a1 = vml.FindCommentShape(0, 0);
            ClassicAssert.IsNotNull(sh_a1);
            ClassicAssert.AreEqual("_x0000_s1025", sh_a1.id);

            CT_Shape sh_b1 = vml.FindCommentShape(0, 1);
            ClassicAssert.IsNotNull(sh_b1);
            ClassicAssert.AreEqual("_x0000_s1026", sh_b1.id);

            CT_Shape sh_c1 = vml.FindCommentShape(0, 2);
            ClassicAssert.IsNull(sh_c1);

            CT_Shape sh_d1 = vml.newCommentShape();
            ClassicAssert.AreEqual("_x0000_s1027", sh_d1.id);
            sh_d1.GetClientDataArray(0).SetRowArray(0, 0);
            sh_d1.GetClientDataArray(0).SetColumnArray(0, 3);
            ClassicAssert.AreSame(sh_d1, vml.FindCommentShape(0, 3));

            //newly created drawing
            XSSFVMLDrawing newVml = new XSSFVMLDrawing();
            ClassicAssert.IsNull(newVml.FindCommentShape(0, 0));

            sh_a1 = newVml.newCommentShape();
            ClassicAssert.AreEqual("_x0000_s1025", sh_a1.id);
            sh_a1.GetClientDataArray(0).SetRowArray(0, 0);
            sh_a1.GetClientDataArray(0).SetColumnArray(0, 1);
            ClassicAssert.AreSame(sh_a1, newVml.FindCommentShape(0, 1));
        }
        [Test]
        public void TestRead()
        {
            XSSFVMLDrawing vml = new XSSFVMLDrawing();

            // Act
            TestDelegate testDelegate = () => vml.Read(POIDataSamples.GetSpreadSheetInstance().OpenResourceAsStream("vmlDrawing1.vml"));

            // Assert
            Assert.DoesNotThrow(testDelegate);
        }
        [Test]
        public void TestRemoveCommentShape()
        {
            XSSFVMLDrawing vml = new XSSFVMLDrawing();
            vml.Read(POIDataSamples.GetSpreadSheetInstance().OpenResourceAsStream("vmlDrawing1.vml"));

            CT_Shape sh_a1 = vml.FindCommentShape(0, 0);
            ClassicAssert.IsNotNull(sh_a1);

            ClassicAssert.IsTrue(vml.RemoveCommentShape(0, 0));
            ClassicAssert.IsNull(vml.FindCommentShape(0, 0));

        }
        [Test]
        public void TestCommentShowsBox()
        {
            XSSFWorkbook wb = new XSSFWorkbook();
            wb.CreateSheet();
            XSSFSheet sheet = (XSSFSheet)wb.GetSheetAt(0);
            XSSFCell cell = (XSSFCell)sheet.CreateRow(0).CreateCell(0);
            XSSFDrawing drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();
            XSSFCreationHelper factory = (XSSFCreationHelper)wb.GetCreationHelper();
            XSSFClientAnchor anchor = (XSSFClientAnchor)factory.CreateClientAnchor();
            anchor.Col1 = cell.ColumnIndex;
            anchor.Col2 = cell.ColumnIndex + 3;
            anchor.Row1 = cell.RowIndex;
            anchor.Row2 = cell.RowIndex + 5;
            XSSFComment comment = (XSSFComment)drawing.CreateCellComment(anchor);
            XSSFRichTextString str = (XSSFRichTextString)factory.CreateRichTextString("this is a comment");
            comment.String = str;
            cell.CellComment = comment;

            XSSFVMLDrawing vml = sheet.GetVMLDrawing(false);
            CT_Shapetype shapetype = null;
            ArrayList items = vml.GetItems();
            foreach(object o in items)
                if(o is CT_Shapetype)
                    shapetype = (CT_Shapetype) o;
            ClassicAssert.AreEqual(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t, shapetype.stroked);
            ClassicAssert.AreEqual(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t, shapetype.filled);

            using(MemoryStream ws = new MemoryStream())
            {
                wb.Write(ws);

                using(MemoryStream rs = new MemoryStream(ws.GetBuffer()))
                {
                    wb = new XSSFWorkbook(rs);
                    sheet = (XSSFSheet) wb.GetSheetAt(0);

                    vml = sheet.GetVMLDrawing(false);
                    shapetype = null;
                    items = vml.GetItems();
                    foreach(object o in items)
                        if(o is CT_Shapetype)
                            shapetype = (CT_Shapetype) o;

                    //wb.Write(new FileStream("comments.xlsx", FileMode.Create));
                    //using (MemoryStream ws2 = new MemoryStream())
                    //{
                    //    vml.Write(ws2);
                    //    throw new System.Exception(System.Text.Encoding.UTF8.GetString(ws2.GetBuffer()));
                    //}

                    ClassicAssert.AreEqual(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t, shapetype.stroked);
                    ClassicAssert.AreEqual(NPOI.OpenXmlFormats.Vml.ST_TrueFalse.t, shapetype.filled);
                }
            }
        }

        [Test]
        public void TestEvilUnclosedBRFixing() {
            XSSFVMLDrawing vml = new XSSFVMLDrawing();
            Stream stream = POIDataSamples.GetOpenXML4JInstance().OpenResourceAsStream("bug-60626.vml");
            try {
                vml.Read(stream);
            } finally {
                stream.Close();
            }
            Regex p = new Regex("<br\\s?/>", RegexOptions.Compiled);
            int count = 0;
            foreach(Object xo in vml.GetItems())
            {
                var t = xo.ToString();
                String[] split = p.Split(t);
                count += split.Length-1;
            }

            ClassicAssert.AreEqual(16, count);
        }
    }
}
