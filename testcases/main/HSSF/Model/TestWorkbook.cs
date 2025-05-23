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

namespace TestCases.HSSF.Model
{
    using System;
    using NUnit.Framework;using NUnit.Framework.Legacy;
    using NPOI.HSSF.Record;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.Formula;
    using NPOI.SS.Formula.Eval;
    using NPOI.SS.Formula.Functions;
    using NPOI.SS.Formula.UDF;
    using TestCases.HSSF.UserModel;
    using NPOI.HSSF.Model;

    /**
     * Unit test for the Workbook class.
     *
     * @author Glen Stampoultzis (glens at apache.org)
     */
    [TestFixture]
    public class TestWorkbook
    {
        [Test]
        public void TestFontStuff()
        {
            HSSFWorkbook hwb = new HSSFWorkbook();
            InternalWorkbook wb = TestHSSFWorkbook.GetInternalWorkbook(hwb);

            ClassicAssert.AreEqual(4, wb.NumberOfFontRecords);
            ClassicAssert.AreEqual(68, wb.Records.Count);

            FontRecord f1 = wb.GetFontRecordAt(0);
            FontRecord f4 = wb.GetFontRecordAt(3);

            ClassicAssert.AreEqual(0, wb.GetFontIndex(f1));
            ClassicAssert.AreEqual(3, wb.GetFontIndex(f4));

            ClassicAssert.AreEqual(f1, wb.GetFontRecordAt(0));
            ClassicAssert.AreEqual(f4, wb.GetFontRecordAt(3));

            // There is no 4! new ones go in at 5

            FontRecord n = wb.CreateNewFont();
            ClassicAssert.AreEqual(69, wb.Records.Count);
            ClassicAssert.AreEqual(5, wb.NumberOfFontRecords);
            ClassicAssert.AreEqual(5, wb.GetFontIndex(n));
            ClassicAssert.AreEqual(n, wb.GetFontRecordAt(5));

            // And another
            FontRecord n6 = wb.CreateNewFont();
            ClassicAssert.AreEqual(70, wb.Records.Count);
            ClassicAssert.AreEqual(6, wb.NumberOfFontRecords);
            ClassicAssert.AreEqual(6, wb.GetFontIndex(n6));
            ClassicAssert.AreEqual(n6, wb.GetFontRecordAt(6));


            // Now remove the one formerly at 5
            ClassicAssert.AreEqual(70, wb.Records.Count);
            wb.RemoveFontRecord(n);

            // Check that 6 has gone to 5
            ClassicAssert.AreEqual(69, wb.Records.Count);
            ClassicAssert.AreEqual(5, wb.NumberOfFontRecords);
            ClassicAssert.AreEqual(5, wb.GetFontIndex(n6));
            ClassicAssert.AreEqual(n6, wb.GetFontRecordAt(5));

            // Check that the earlier ones are unChanged
            ClassicAssert.AreEqual(0, wb.GetFontIndex(f1));
            ClassicAssert.AreEqual(3, wb.GetFontIndex(f4));
            ClassicAssert.AreEqual(f1, wb.GetFontRecordAt(0));
            ClassicAssert.AreEqual(f4, wb.GetFontRecordAt(3));

            // Finally, add another one
            FontRecord n7 = wb.CreateNewFont();
            ClassicAssert.AreEqual(70, wb.Records.Count);
            ClassicAssert.AreEqual(6, wb.NumberOfFontRecords);
            ClassicAssert.AreEqual(6, wb.GetFontIndex(n7));
            ClassicAssert.AreEqual(n7, wb.GetFontRecordAt(6));

            hwb.Close();
        }
        private class FreeRefFunction1 : FreeRefFunction
        {
            #region FreeRefFunction ��Ա

            public ValueEval Evaluate(ValueEval[] args, OperationEvaluationContext ec)
            {
                throw new NotImplementedException("not implemented");
            }

            #endregion
        }
        [Test]
        public void TestAddNameX()
        {
            HSSFWorkbook hwb = new HSSFWorkbook();
            InternalWorkbook wb = TestHSSFWorkbook.GetInternalWorkbook(hwb);
            ClassicAssert.IsNotNull(wb.GetNameXPtg("ISODD", UDFFinder.GetDefault()));

            FreeRefFunction1 NotImplemented = new FreeRefFunction1();

            /**
             * register the two test UDFs in a UDF Finder, to be passed to the Evaluator
             */
            UDFFinder udff1 = new DefaultUDFFinder(new String[] { "myFunc", },
                    new FreeRefFunction[] { NotImplemented });
            UDFFinder udff2 = new DefaultUDFFinder(new String[] { "myFunc2", },
                    new FreeRefFunction[] { NotImplemented });
            UDFFinder udff = new AggregatingUDFFinder(udff1, udff2);
            ClassicAssert.IsNotNull(wb.GetNameXPtg("myFunc", udff));
            ClassicAssert.IsNotNull(wb.GetNameXPtg("myFunc2", udff));

            ClassicAssert.IsNull(wb.GetNameXPtg("myFunc3", udff));  // myFunc3 is unknown

            hwb.Close();
        }
        [Test]
        public void TestRecalcId()
        {
            HSSFWorkbook wb = new HSSFWorkbook();
            ClassicAssert.IsFalse(wb.ForceFormulaRecalculation);

            InternalWorkbook iwb = TestHSSFWorkbook.GetInternalWorkbook(wb);
            int countryPos = iwb.FindFirstRecordLocBySid(CountryRecord.sid);
            ClassicAssert.IsTrue(countryPos != -1);
            // RecalcIdRecord is not present in new workbooks
            ClassicAssert.AreEqual(null, iwb.FindFirstRecordBySid(RecalcIdRecord.sid));
            RecalcIdRecord record = iwb.RecalcId;
            ClassicAssert.IsNotNull(record);
            ClassicAssert.AreSame(record, iwb.RecalcId);

            ClassicAssert.AreSame(record, iwb.FindFirstRecordBySid(RecalcIdRecord.sid));
            ClassicAssert.AreEqual(countryPos + 1, iwb.FindFirstRecordLocBySid(RecalcIdRecord.sid));

            record.EngineId = (/*setter*/100);
            ClassicAssert.AreEqual(100, record.EngineId);
            ClassicAssert.IsTrue(wb.ForceFormulaRecalculation);

            wb.ForceFormulaRecalculation = (/*setter*/true); // resets the EngineId flag to zero
            ClassicAssert.AreEqual(0, record.EngineId);
            ClassicAssert.IsFalse(wb.ForceFormulaRecalculation);

            wb.Close();
        }
    }

}