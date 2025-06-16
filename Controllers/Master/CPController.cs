using System;
using System.Collections.Generic;
using System.Linq;
using cms_api.Extension;
using cms_api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace master_api.Controllers
{
    [Route("[controller]")]
    public class CPController : Controller
    {
        public CPController() { }

        // POST /initial
        [HttpGet("fixRegisterStatus")]
        public ActionResult<Response> FixRegisterStatus()
        {
            var colRegister = new Database().MongoClient("register");
            var filterRegister = Builders<BsonDocument>.Filter.Eq("countUnit", "[]") & Builders<BsonDocument>.Filter.Eq("status", "V");
            var updateAll = Builders<BsonDocument>.Update.Set("status", "N");
            colRegister.UpdateMany(filterRegister, updateAll);
            return new Response { status = "S", message = "success", objectData = "success" };
        }

        // POST /initial
        [HttpGet("fixRegisterStatus2")]
        public ActionResult<Response> FixRegisterStatus2()
        {
            var colRegister = new Database().MongoClient<Register>("register");
            //var filterRegister = Builders<Register>.Filter.Ne("countUnit", "") & Builders<Register>.Filter.Eq("status", "N");
            var doc = colRegister.Find(_ => true).Project(c => new { c.code, c.countUnit }).ToList();

            doc.ForEach(c =>
            {
                if (c.countUnit != null)
                {
                    List<CountUnit> countUnitValue = JsonConvert.DeserializeObject<List<CountUnit>>(c.countUnit); // count unit from value
                    countUnitValue.ForEach(cc =>
                    {
                        cc.status = "A";
                    });

                    var filterByRegister = Builders<Register>.Filter.Eq("code", c.code);
                    var updateAll = Builders<Register>.Update.Set("status", "A").Set("countUnit", JsonConvert.SerializeObject(countUnitValue));
                    colRegister.UpdateOne(filterByRegister, updateAll);
                }
            });
            
            return new Response { status = "S", message = "success", objectData = "success" };
        }

        // POST /initial
        [HttpGet("initial")]
        public ActionResult<Response> Initial()
        {
            var colUpdateAll = new Database().MongoClient("organization");
            var updateAll = Builders<BsonDocument>.Update.Set("cpCode", "").Set("cpTitle", "");
            colUpdateAll.UpdateMany(_ => true, updateAll);

            var col = new Database().MongoClient<Identity>("organization");
            var filter = Builders<Identity>.Filter.Eq("lv0", "20201126102446-653-959");
            var docs = col.Find(filter).Project(c => new { c.code, c.title, c.codeShort }).ToList();
            docs.ForEach(c =>
            {
                switch (c.codeShort)
                {
                    case "01":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "ชัยนาท")
                                    | Builders<BsonDocument>.Filter.Eq("title", "พระนครศรีอยุธยา")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ลพบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สระบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สิงห์บุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "อ่างทอง"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                       
                        break;
                    case "02":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "นนทบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ปทุมธานี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "นครปฐม")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สมุทรปราการ"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "03":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "กาญจนบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ราชบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สุพรรณบุรี"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "04":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "ประจวบคีรีขันธ์")
                                    | Builders<BsonDocument>.Filter.Eq("title", "เพชรบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สมุทรสงคราม")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สมุทรสาคร"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "05":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "ชุมพร")
                                    | Builders<BsonDocument>.Filter.Eq("title", "นครศรีธรรมราช")
                                    | Builders<BsonDocument>.Filter.Eq("title", "พัทลุง")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สุราษฎร์ธานี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สงขลา"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "06":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "กระบี่")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ตรัง")
                                    | Builders<BsonDocument>.Filter.Eq("title", "พังงา")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ภูเก็ต")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ระนอง")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สตูล"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "07":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "นราธิวาส")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ปัตตานี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ยะลา"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "08":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "ฉะเชิงเทรา")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ชลบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ระยอง"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "09":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "จันทบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ตราด")
                                    | Builders<BsonDocument>.Filter.Eq("title", "นครนายก")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ปราจีนบุรี")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สระแก้ว"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "10":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "บึงกาฬ")
                                    | Builders<BsonDocument>.Filter.Eq("title", "เลย")
                                    | Builders<BsonDocument>.Filter.Eq("title", "หนองคาย")
                                    | Builders<BsonDocument>.Filter.Eq("title", "หนองบัวลำภู")
                                    | Builders<BsonDocument>.Filter.Eq("title", "อุดรธานี"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "11":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "นครพนม")
                                    | Builders<BsonDocument>.Filter.Eq("title", "มุกดาหาร")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สกลนคร"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "12":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "กาฬสินธุ์")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ขอนแก่น")
                                    | Builders<BsonDocument>.Filter.Eq("title", "มหาสารคาม")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ร้อยเอ็ด"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "13":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "ชัยภูมิ")
                                    | Builders<BsonDocument>.Filter.Eq("title", "นครราชสีมา")
                                    | Builders<BsonDocument>.Filter.Eq("title", "บุรีรัมย์")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สุรินทร์"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "14":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "ยโสธร")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ศรีสะเกษ")
                                    | Builders<BsonDocument>.Filter.Eq("title", "อำนาจเจริญ")
                                    | Builders<BsonDocument>.Filter.Eq("title", "อุบลราชธานี"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "15":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "เชียงใหม่")
                                    | Builders<BsonDocument>.Filter.Eq("title", "แม่ฮ่องสอน")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ลำปาง")
                                    | Builders<BsonDocument>.Filter.Eq("title", "ลำพูน"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "16":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "เชียงราย")
                                    | Builders<BsonDocument>.Filter.Eq("title", "น่าน")
                                    | Builders<BsonDocument>.Filter.Eq("title", "พะเยา")
                                    | Builders<BsonDocument>.Filter.Eq("title", "แพร่"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "17":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "ตาก")
                                    | Builders<BsonDocument>.Filter.Eq("title", "พิษณุโลก")
                                    | Builders<BsonDocument>.Filter.Eq("title", "สุโขทัย")
                                    | Builders<BsonDocument>.Filter.Eq("title", "เพชรบูรณ์")
                                    | Builders<BsonDocument>.Filter.Eq("title", "อุตรดิตถ์"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "18":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "กำแพงเพชร")
                                    | Builders<BsonDocument>.Filter.Eq("title", "นครสวรรค์")
                                    | Builders<BsonDocument>.Filter.Eq("title", "พิจิตร")
                                    | Builders<BsonDocument>.Filter.Eq("title", "อุทัยธานี"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    case "19":
                        {
                            var colUpdate = new Database().MongoClient("organization");
                            //{ category: "lv2", lv0: "20200828135717-103-723"}
                            var filterUpdate = Builders<BsonDocument>.Filter.Eq("category", "lv2")
                                & Builders<BsonDocument>.Filter.Eq("lv0", "20200828135717-103-723")
                                & (Builders<BsonDocument>.Filter.Eq("title", "กรุงเทพมหานคร"));
                            var update = Builders<BsonDocument>.Update.Set("cpCode", c.code).Set("cpTitle", c.title);
                            colUpdate.UpdateMany(filterUpdate, update);
                        }
                        break;
                    default:
                        break;
                }
            });
            return new Response { status = "S", message = "success", objectData = docs };
        }

        // POST /initial
        [HttpPost("initialCenter")]
        public ActionResult<Response> InitialProvince([FromBody] object value)
        {
            var col = new Database().MongoClient("mCenter");
            var doc = new BsonDocument();
            var model = new List<Identity>
            {
                new Identity { sequence = 0,  code = "", title = "สำนักงานสัตวแพทยสภา", titleEN = "Vetcouncil", titleShort = "VET",titleShortEN = "VET" },
                new Identity { sequence = 1,  code = "001", title = "ศูนย์การศึกษาต่อเนื่องทางสัตวแพทย์", titleEN = "ศูนย์การศึกษาต่อเนื่องทางสัตวแพทย์", titleShort = "CE",titleShortEN = "CE" },
                new Identity { sequence = 2, code = "002", title = "ศูนย์ประเมินความรู้ความสามารถขั้นพื้นฐานของการประกอบวิชาชีพการสัตวแพทย์ (CVCA)", titleEN = "ศูนย์ประเมินความรู้ความสามารถขั้นพื้นฐานของการประกอบวิชาชีพการสัตวแพทย์ (CVCA)",titleShort = "CVCA",titleShortEN = "CVCA" },
                new Identity { sequence = 3, code = "003", title = "วิทยาลัยวิชาชีพการสัตวแพทย์ชำนาญการแห่งประเทศไทย (CVST)", titleEN = "วิทยาลัยวิชาชีพการสัตวแพทย์ชำนาญการแห่งประเทศไทย (CVST)", titleShort="CVST",titleShortEN="CVST" },
                new Identity { sequence = 4, code = "004", title = "มาตรฐานวิชาชีพการสัตวแพทย์", titleEN = "มาตรฐานวิชาชีพการสัตวแพทย์", titleShort = "มาตรฐานวิชาชีพฯ", titleShortEN = "มาตรฐานวิชาชีพฯ" },
                new Identity { sequence = 5, code = "005", title = "จรรยาบรรณ", titleEN = "จรรยาบรรณ", titleShort = "จรรยาบรรณ",titleShortEN = "จรรยาบรรณ" },
                new Identity { sequence = 6, code = "006", title = "อนุกรรมการต่างประเทศ", titleEN = "อนุกรรมการต่างประเทศ", titleShort = "อนุกรรมการต่างประเทศ", titleShortEN = "อนุกรรมการต่างประเทศ" },
            };

            try
            {
                model.ForEach(c =>
                {
                    var filter = Builders<BsonDocument>.Filter.Eq("code", c.code);
                    if (!col.Find(filter).Any())
                    {
                        doc = new BsonDocument
                    {
                        { "code", c.code },
                        { "sequence", c.sequence },
                        { "title", c.title },
                        { "titleEN", c.titleEN },
                        { "titleShort", c.titleShort },
                        { "titleShortEN", c.titleShortEN },
                        { "createBy", "system" },
                        { "createDate", DateTime.Now.toStringFromDate() },
                        { "createTime", DateTime.Now.toTimeStringFromDate() },
                        { "updateBy", "system" },
                        { "updateDate", DateTime.Now.toStringFromDate() },
                        { "updateTime", DateTime.Now.toTimeStringFromDate() },
                        { "docDate", DateTime.Now.Date.AddHours(7) },
                        { "docTime", DateTime.Now.toTimeStringFromDate() },
                        { "isActive", true }
                    };
                        col.InsertOne(doc);
                    }
                });

                return new Response { status = "S", message = "success" };
            }
            catch (Exception ex)
            {
                return new Response { status = "S", message = ex.Message };
            }
        }
    }
}