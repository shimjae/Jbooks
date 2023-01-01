using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JBooks_ESM
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            work wk = new work();
            try
            {

            


                Console.WriteLine("=============  상품등록 시작  =============");

                string Authorization = wk.InitAuth();


                //Console.WriteLine("=============  기간연장 시작  =============");
                //DataTable dtPeriodList1 = new DataTable();
                //dtPeriodList1 = wk.GetPeriodGoodsList();
                //foreach (DataRow dr in dtPeriodList1.Rows)
                //{
                //    try
                //    {
                //        DataTable Dtdetail = wk.GetGoodsDetailList(dr["pk_id"].ToString());
                //        wk.PeriodGoods(Authorization, wk.SetBookr(Dtdetail).Rows[0]);
                //       // wk.DeleteGoods(Authorization, wk.SetBookr(Dtdetail).Rows[0]);

                //    }
                //    catch (Exception ex)
                //    {
                //        continue;
                //    }
                //}

                Console.WriteLine("=============  상품업데이트 시작  =============");
                DataTable dtUptList = new DataTable();
                dtUptList = wk.GetUptGoodsList();
                foreach (DataRow dr in dtUptList.Rows)
                {
                    try
                    {
                        DataTable Dtdetail = wk.GetGoodsDetailList(dr["pk_id"].ToString());
                        Thread.Sleep(1000);
                        wk.UptGoods(Authorization, wk.SetBookr(Dtdetail).Rows[0]);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }

                DataTable dtGoodsList = new DataTable();
                dtGoodsList = wk.GetGoodsList();
                Console.WriteLine("=============  1.디비 시작  =============" + dtGoodsList.Rows.Count.ToString());
                foreach (DataRow dr in dtGoodsList.Rows)
                {
                    try
                    {
                        DataTable dtMain = wk.GetCntGoodsList(dr["pk_id"].ToString());
                        Console.WriteLine("=============  2.디비 시작  =============" + dr["pk_id"].ToString());
                        int icnt = dtMain.Rows.Count;
                        if (icnt < 1)
                        {

                            DataTable Dtdetail = wk.GetGoodsDetailList(dr["pk_id"].ToString());
                            // wk.regCategory(Authorization,wk.SetBookr(Dtdetail).Rows[0]);
                            //wk.regEsmCategory(Authorization);
                            //JsonObjectCollection obj = wk.regEsmBrandInfo(Authorization,"마술피리");
                            DataRow rows = wk.SetBookr(Dtdetail).Rows[0];
                            if (rows["image_name"].ToString() != "")
                            {
                                using (WebClient client = new WebClient())
                                {
                                    try
                                    {
                                        client.DownloadFile(rows["image_url"].ToString(), @"D:\Temp\Image\" + rows["pk_id"].ToString() + ".jpg");
                                    }
                                    catch(Exception ex)
                                    {
                                        wk.InsertError(rows, "이미지600");
                                    }
                                    
                                }

                                Bitmap image1 = new Bitmap(@"D:\Temp\Image\" + rows["pk_id"].ToString() + ".jpg", true);
                                if (image1.Width >= 600)
                                {
                                    Thread.Sleep(1000);
                                    wk.RegGoods(Authorization, rows);
                                    image1.Dispose();
                                    var tempfile = new FileInfo(@"D:\Temp\Image\" + rows["pk_id"].ToString() + ".jpg");
                                    tempfile.Delete();
                                }
                                else
                                {
                                    Console.WriteLine(rows["pk_id"].ToString() + "이미지사이즈에러");
                                    wk.InsertError(rows, "이미지600");
                                }



                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        continue;
                    }
                }
               

                Console.WriteLine("=============  기간연장 시작  =============");
                DataTable dtPeriodList = new DataTable();
                dtPeriodList = wk.GetPeriodGoodsList();
                foreach (DataRow dr in dtPeriodList.Rows)
                {
                    try
                    {
                        DataTable Dtdetail = wk.GetGoodsDetailList(dr["pk_id"].ToString());
                        wk.PeriodGoods(Authorization, wk.SetBookr(Dtdetail).Rows[0]);

                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());

            }
        }

        public class work
        {
            public DataTable GetUptGoodsList()
            {

                DataTable dtGoods = new DataTable();
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";

                OracleConnection OraConn = new OracleConnection(_strConn);

                //sql에 저장된 데이터베이스 정보로 연결

                OraConn.Open();//디비 오픈

                OracleDataAdapter oda = new OracleDataAdapter();//어댑터 생성자



                oda.SelectCommand = new OracleCommand(@"  SELECT  A.PK_ID, A.BK_CD, A.BK_NM, A.PUB_CD, A.PUB_NM, A.PRICE, A.DIS_PRICE, A.MIMG_FNM, A.TAX_TYPE, f_mj_amt(A.BK_CD,15) MARGIN_AMOUNT,
                    A.BK_INTRO, A.BK_CONT, A.AUTHOR, A.BK_SIZE, A.PRESS_DT, A.BK_PAGE, A.DIS_RATE, A.AWARD_HST, A.PJ_GB, A.SHIP_COST, A.MEDIA_FILE,                
                    'http://image.jbookshop.co.kr/Big/' || mimg_fnm || '.jpg' AS IMG_URL,B.GOODSNO
                FROM EBOOKCD A , EGOODS_ESM B
                WHERE A.PK_ID = B.PK_ID AND A.UPD_DATE > TO_CHAR(sysdate-1,'yyyymmddhh24miss')", OraConn);

                //    oda.SelectCommand = new OracleCommand(@"  SELECT  A.PK_ID, A.BK_CD, A.BK_NM, A.PUB_CD, A.PUB_NM, A.PRICE, A.DIS_PRICE, A.MIMG_FNM, A.TAX_TYPE, f_mj_amt(A.BK_CD,15) MARGIN_AMOUNT,
                //    A.BK_INTRO, A.BK_CONT, A.AUTHOR, A.BK_SIZE, A.PRESS_DT, A.BK_PAGE, A.DIS_RATE, A.AWARD_HST, A.PJ_GB, A.SHIP_COST, A.MEDIA_FILE,                
                //    (SELECT C.IMAGE FROM KYOBO C WHERE A.BK_CD=C.BK_CD) IMG_URL ,B.GOODSNO
                //FROM EBOOKCD A , EGOODS_ESM B
                //WHERE A.PK_ID = B.PK_ID AND A.PJ_GB <> B.UPD_GB", OraConn);
                oda.Fill(dtGoods);

                OraConn.Close();

                return dtGoods;
            }

            public DataTable GetPeriodGoodsList()
            {

                DataTable dtGoods = new DataTable();
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";

                OracleConnection OraConn = new OracleConnection(_strConn);

                //sql에 저장된 데이터베이스 정보로 연결

                OraConn.Open();//디비 오픈

                OracleDataAdapter oda = new OracleDataAdapter();//어댑터 생성자


                //oda.SelectCommand = new OracleCommand("select * from VX_IFBIBLIO_ONLINE WHERE isbn13='9788956056722'", OraConn);
                oda.SelectCommand = new OracleCommand(@"  SELECT  A.PK_ID, A.BK_CD, A.BK_NM, A.PUB_CD, A.PUB_NM, A.PRICE, A.DIS_PRICE, A.MIMG_FNM, A.TAX_TYPE, f_mj_amt(A.BK_CD,15) MARGIN_AMOUNT,
                A.BK_INTRO, A.BK_CONT, A.AUTHOR, A.BK_SIZE, A.PRESS_DT, A.BK_PAGE, A.DIS_RATE, A.AWARD_HST, A.PJ_GB, A.SHIP_COST, A.MEDIA_FILE,                
                (SELECT C.IMAGE FROM KYOBO C WHERE A.BK_CD=C.BK_CD) IMG_URL ,B.GOODSNO
            FROM EBOOKCD A , EGOODS_ESM B
            WHERE A.PK_ID = B.PK_ID
            AND TO_DATE(B.EXPIRED_DATE) - SYSDATE < 10", OraConn);

                oda.Fill(dtGoods);

                OraConn.Close();

                return dtGoods;
            }

            public void PeriodGoods(string Authorization, DataRow dr)
            {
                try
                {
                    Console.WriteLine("=============  기간연장 : " + dr["name"].ToString() + "  =============");
                    JsonObjectCollection isSell = new JsonObjectCollection("isSell");
                    if (dr["PJ_GB"].ToString() == "C")
                    {
                        isSell.Add(new JsonBooleanValue("gmkt", false));
                       
                    }
                    else
                    {
                        isSell.Add(new JsonBooleanValue("gmkt", false));
                       
                    }
                    JsonObjectCollection itemBasicInfo = new JsonObjectCollection("itemBasicInfo");
                    JsonObjectCollection price = new JsonObjectCollection("price");
                    price.Add(new JsonNumericValue("gmkt", Convert.ToInt32(dr["price"].ToString())));
                 

                    JsonObjectCollection Stock = new JsonObjectCollection("Stock");
                    if (dr["PJ_GB"].ToString() == "C")
                    {
                        Stock.Add(new JsonNumericValue("gmkt", 0));
                     
                    }
                    else
                    {
                        Stock.Add(new JsonNumericValue("gmkt", 0));
                     
                    }

                    JsonObjectCollection SellingPeriod = new JsonObjectCollection("SellingPeriod");
                    SellingPeriod.Add(new JsonNumericValue("gmkt", 365));
                   

                    itemBasicInfo.Add(price);
                    itemBasicInfo.Add(Stock);
                    itemBasicInfo.Add(SellingPeriod);

                    JsonObjectCollection obj = new JsonObjectCollection();
                    obj.Add(isSell);
                    obj.Add(itemBasicInfo);

                    string response = "";
                    using (WebClient client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        client.Headers.Add("Content-Type", "application/json");
                        client.Headers.Add("Authorization", Authorization);
                        response = client.UploadString("https://sa2.esmplus.com/item/v1/goods/"+dr["GOODSNO"].ToString()+"/sell-status", "PUT", obj.ToString());

                        JsonTextParser parser = new JsonTextParser();
                        JsonObjectCollection jc = (JsonObjectCollection)parser.Parse(response);

                        if (response.IndexOf("goodsNo") > -1)
                        {
                            UptPeriod(dr);
                        }else
                        {
                            string Message = jc["Message"].GetValue().ToString();
                        }

                    }

                }
                catch (Exception ex)
                {

                }




            }

            public void UptPeriod(DataRow dr)
            {
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";
                // 오라클 연결
                OracleConnection conn = new OracleConnection(_strConn);

                try
                {
                    conn.Open();

                    // 명령 객체 생성
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;

                    // SQL문 지정 및 INSERT 실행
                    cmd.CommandText = @"UPDATE EGOODS_ESM SET  UPD_DATE = TO_CHAR(sysdate,'yyyymmddhh24miss') ,EXPIRED_DATE = TO_CHAR(TO_DATE(EXPIRED_DATE, 'YYYY-MM-DD') + 365, 'YYYY-MM-DD') WHERE GOODSNO= '" + dr["GOODSNO"].ToString() + "'";

                    cmd.ExecuteNonQuery();


                }
                catch (Exception ex)
                {

                }
                finally
                {
                    conn.Close();
                }

            }



            public void DeleteGoods(string Authorization, DataRow dr)
            {
                WebRequest request = WebRequest.Create("https://sa2.esmplus.com/item/v1/goods/" + dr["GOODSNO"].ToString());
                request.Method = "DELETE";

                request.Headers.Add("Authorization", Authorization);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stReadData = response.GetResponseStream();
                StreamReader srReadData = new StreamReader(stReadData, Encoding.Default, true);

                // 응답 Stream -> 응답 String 변환
                string strResult = srReadData.ReadToEnd();

            }
            public void UptGoods(string Authorization, DataRow dr)
            {
                string ErrMsg = "";
                try
                {
                    Console.WriteLine("=============  상품수정 : " + dr["name"].ToString() + "  =============");
                    JsonObjectCollection isSell = new JsonObjectCollection("isSell");
                    if (dr["PJ_GB"].ToString() == "C")
                    {
                        isSell.Add(new JsonBooleanValue("gmkt", true));
                        isSell.Add(new JsonBooleanValue("iac", true));
                    }
                    else
                    {
                        isSell.Add(new JsonBooleanValue("gmkt", false));
                        isSell.Add(new JsonBooleanValue("iac", false));
                    }


                    JsonObjectCollection itemBasicInfo = new JsonObjectCollection("itemBasicInfo");
                    JsonObjectCollection goodsName = new JsonObjectCollection("goodsName");
                    goodsName.Add(new JsonStringValue("kor", dr["name"].ToString()));
                    itemBasicInfo.Add(goodsName);

                    JsonObjectCollection category = new JsonObjectCollection("category");
                    JsonArrayCollection arr_site = new JsonArrayCollection("site");
                    JsonArrayCollection arr_esm = new JsonArrayCollection("esm");

                    JsonObjectCollection siteCode_1 = new JsonObjectCollection();
                    siteCode_1.Add(new JsonNumericValue("siteType", 1)); //1.옥션 2.G마켓
                    siteCode_1.Add(new JsonStringValue("catCode", dr["CAT_AUCTION"].ToString()));
                    JsonObjectCollection siteCode_2 = new JsonObjectCollection();
                    siteCode_2.Add(new JsonNumericValue("siteType", 2)); //1.옥션 2.G마켓
                    siteCode_2.Add(new JsonStringValue("catCode", dr["CAT_GMARKET"].ToString()));

                    JsonObjectCollection esm_catCode = new JsonObjectCollection("esm");
                    esm_catCode.Add(new JsonStringValue("catCode", dr["CAT_ESM"].ToString()));

                    arr_site.Add(siteCode_1);
                    arr_site.Add(siteCode_2);
                    category.Add(arr_site);
                    category.Add(esm_catCode);
                    itemBasicInfo.Add(category);

                    JsonObjectCollection book = new JsonObjectCollection("book");
                    book.Add(new JsonBooleanValue("isUseIsbnCode", true));
                    book.Add(new JsonStringValue("isbnCode", dr["barcode"].ToString()));
                    book.Add(new JsonNumericValue("price", Convert.ToInt32(dr["price"].ToString())));
                    itemBasicInfo.Add(book);

                    JsonObjectCollection catalog = new JsonObjectCollection("catalog");
                    JsonObjectCollection brand = GetEsmBrandInfo(Authorization, dr["publisher_name"].ToString());

                    catalog.Add(new JsonStringValue("modelName", dr["publisher_name"].ToString()));

                    if (brand["brandNo"].GetValue().ToString() != "0")
                    {
                        catalog.Add(new JsonNumericValue("brandNo", Convert.ToInt32(brand["brandNo"].GetValue().ToString())));
                    }

                    catalog.Add(new JsonStringValue("barCode", dr["barcode"].ToString()));
                    itemBasicInfo.Add(catalog);

                    JsonObjectCollection itemAddtionalInfo = new JsonObjectCollection("itemAddtionalInfo");
                    JsonObjectCollection buyableQuantity = new JsonObjectCollection("buyableQuantity");
                    buyableQuantity.Add(new JsonNumericValue("type", 0));
                    buyableQuantity.Add(new JsonNumericValue("qty", 999));
                    itemAddtionalInfo.Add(buyableQuantity);

                    JsonObjectCollection price = new JsonObjectCollection("price");
                    price.Add(new JsonNumericValue("Gmkt", Convert.ToInt32(dr["sale_price"].ToString())));
                    price.Add(new JsonNumericValue("Iac", Convert.ToInt32(dr["sale_price"].ToString())));
                    itemAddtionalInfo.Add(price);

                    JsonObjectCollection stock = new JsonObjectCollection("stock");
                    stock.Add(new JsonNumericValue("Gmkt", 9999));
                    stock.Add(new JsonNumericValue("Iac", 9999));
                    itemAddtionalInfo.Add(stock);

                    JsonObjectCollection sellingPeriod = new JsonObjectCollection("sellingPeriod");
                    sellingPeriod.Add(new JsonNumericValue("Gmkt", 0)); //상품 수정시, 판매기간 0입력 가능하나 0 입력하면 기존 판매기간이 유지
                    sellingPeriod.Add(new JsonNumericValue("Iac", 0));
                    itemAddtionalInfo.Add(sellingPeriod);


                    itemAddtionalInfo.Add(new JsonStringValue("managedCode", dr["pk_id"].ToString()));

                    JsonObjectCollection recommendedOpts = new JsonObjectCollection("recommendedOpts");
                    recommendedOpts.Add(new JsonNumericValue("type", 0));
                    itemAddtionalInfo.Add(recommendedOpts);

                    JsonObjectCollection orderOpts = new JsonObjectCollection("orderOpts");
                    orderOpts.Add(new JsonNumericValue("type", 0));
                    itemAddtionalInfo.Add(orderOpts);

                    itemAddtionalInfo.Add(new JsonStringValue("manufacturedDate", dr["press_date"].ToString()));  //제조일 YYYY-MM-DD 형태로 입력

                    JsonObjectCollection shipping = new JsonObjectCollection("shipping");
                    shipping.Add(new JsonNumericValue("type", 1));
                    shipping.Add(new JsonNumericValue("companyNo", 10013));  //택배사코드  10001 대한통운 , 10007 한진,10008 롯데,10013 CJ

                    JsonObjectCollection policy = new JsonObjectCollection("policy");
                    policy.Add(new JsonNumericValue("placeNo", 42357));  //출하지번호
                    policy.Add(new JsonNumericValue("feeType", 1));

                    JsonObjectCollection bundle = new JsonObjectCollection("bundle");
                    //묶음배송비정책번호
                    if (Convert.ToInt32(dr["MARGIN_AMOUNT"].ToString()) >= 200)
                    {
                        bundle.Add(new JsonNumericValue("deliveryTmplId", 4066482));
                    }
                    else
                    {
                        int sale_price = Convert.ToInt32(dr["sale_price"].ToString());

                   
                        if (sale_price < 10000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2557321));
                        }
                        else if (sale_price < 15000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2543515));
                        }
                        else if (sale_price >= 15000 && sale_price < 20000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 171526));
                        }
                        else if (sale_price >= 20000 && sale_price < 25000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 3239986));
                        }
                        else if (sale_price >= 25000 && sale_price < 30000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 3239985));
                        }
                        else if (sale_price >= 30000 && sale_price < 35000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 3984347));
                        }
                        else if (sale_price >= 35000 && sale_price < 40000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2544645));
                        }
                        else if (sale_price >= 40000 && sale_price < 45000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 42130208));
                        }
                        else if (sale_price >= 45000 && sale_price < 50000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2810772));
                        }
                        else if (sale_price >= 50000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2810764));
                        }


                    }
                    policy.Add(bundle);
                    shipping.Add(policy);

                    JsonObjectCollection returnAndExchange = new JsonObjectCollection("returnAndExchange");
                    returnAndExchange.Add(new JsonNumericValue("addrNo", 139719));
                    //returnAndExchange.Add(new JsonNumericValue("shippingCompany", 10013));
                    returnAndExchange.Add(new JsonNumericValue("fee", 2500));
                    shipping.Add(returnAndExchange);

                    JsonObjectCollection dispatchPolicyNo = new JsonObjectCollection("dispatchPolicyNo");
                    dispatchPolicyNo.Add(new JsonNumericValue("gmkt", 243245));
                    dispatchPolicyNo.Add(new JsonNumericValue("iac", 243244));
                    shipping.Add(dispatchPolicyNo);



                    JsonObjectCollection visitAndTake = new JsonObjectCollection("visitAndTake");
                    visitAndTake.Add(new JsonBooleanValue("isUse", false));
                    shipping.Add(visitAndTake);

                    JsonObjectCollection quickService = new JsonObjectCollection("quickService");
                    quickService.Add(new JsonBooleanValue("isUse", false));
                    shipping.Add(quickService);

                    shipping.Add(new JsonStringValue("backwoodsDeliveryYn", "Y"));

                    itemAddtionalInfo.Add(shipping);


                    JsonObjectCollection officialNotice = new JsonObjectCollection("officialNotice");  //고시
                    officialNotice.Add(new JsonNumericValue("officialNoticeNo", 26));
                    JsonArrayCollection arr_details = new JsonArrayCollection("details");
                    JsonObjectCollection details_1 = new JsonObjectCollection();
                    details_1.Add(new JsonStringValue("officialNoticeItemelementCode", "26-1"));
                    details_1.Add(new JsonStringValue("value", dr["name"].ToString()));
                    details_1.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_1);

                    JsonObjectCollection details_2 = new JsonObjectCollection();
                    details_2.Add(new JsonStringValue("officialNoticeItemelementCode", "26-2"));
                    details_2.Add(new JsonStringValue("value", dr["author"].ToString() + "/" + dr["publisher_name"].ToString()));
                    details_2.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_2);


                    JsonObjectCollection details_3 = new JsonObjectCollection();
                    details_3.Add(new JsonStringValue("officialNoticeItemelementCode", "26-3"));
                    details_3.Add(new JsonStringValue("value", dr["size"].ToString() != "" ? dr["size"].ToString() : "상품상세참조"));
                    details_3.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_3);


                    JsonObjectCollection details_4 = new JsonObjectCollection();
                    details_4.Add(new JsonStringValue("officialNoticeItemelementCode", "26-4"));
                    details_4.Add(new JsonStringValue("value", dr["page"].ToString() != "" ? dr["page"].ToString() : "상품상세참조"));
                    details_4.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_4);

                    JsonObjectCollection details_5 = new JsonObjectCollection();
                    details_5.Add(new JsonStringValue("officialNoticeItemelementCode", "26-5"));
                    details_5.Add(new JsonStringValue("value", "상품상세참조"));
                    details_5.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_5);

                    JsonObjectCollection details_6 = new JsonObjectCollection();
                    details_6.Add(new JsonStringValue("officialNoticeItemelementCode", "26-6"));
                    details_6.Add(new JsonStringValue("value", dr["press_date"].ToString() != "" ? dr["press_date"].ToString() : "상품상세참조"));
                    details_6.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_6);


                    JsonObjectCollection details_7 = new JsonObjectCollection();
                    details_7.Add(new JsonStringValue("officialNoticeItemelementCode", "26-7"));
                    details_7.Add(new JsonStringValue("value", "상품상세참조"));
                    details_7.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_7);


                    JsonObjectCollection details_8 = new JsonObjectCollection();
                    details_8.Add(new JsonStringValue("officialNoticeItemelementCode", "26-8"));
                    details_8.Add(new JsonStringValue("value", "2일"));
                    details_8.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_8);

                    officialNotice.Add(arr_details);
                    itemAddtionalInfo.Add(officialNotice);

                    itemAddtionalInfo.Add(new JsonBooleanValue("isAdultProduct", false));
                    itemAddtionalInfo.Add(new JsonBooleanValue("isYouthNotAvailable", false));
                    itemAddtionalInfo.Add(new JsonBooleanValue("isVatFree", dr["tax_type"].ToString() == "N" ? true : false));


                    if (dr["KIDSAUTH"].ToString() !="0")
                    {
                        JsonObjectCollection certInfo = new JsonObjectCollection("certInfo");
                        JsonObjectCollection safetyCerts = new JsonObjectCollection("safetyCerts");
                        JsonObjectCollection child = new JsonObjectCollection("child");
                        JsonArrayCollection arr_child = new JsonArrayCollection("details");
                        child.Add(arr_child);
                        child.Add(new JsonNumericValue("type", 2));


                        safetyCerts.Add(child);
                        certInfo.Add(safetyCerts);
                        itemAddtionalInfo.Add(certInfo);

                    }


                    JsonObjectCollection images = new JsonObjectCollection("images");
                    images.Add(new JsonStringValue("basicImgURL", dr["image_url"].ToString()));
                    //images.Add(new JsonStringValue("addtionalImg1URL", dr["image_url"].ToString()));
                    itemAddtionalInfo.Add(images);

                    JsonObjectCollection descriptions = new JsonObjectCollection("descriptions");
                    JsonObjectCollection kor = new JsonObjectCollection("kor");
                    kor.Add(new JsonNumericValue("type", 2));
                    kor.Add(new JsonStringValue("html", GetContent(dr)));
                    descriptions.Add(kor);
                    itemAddtionalInfo.Add(descriptions);


                    JsonObjectCollection addonService = new JsonObjectCollection("addonService");
                    JsonArrayCollection addonServiceList = new JsonArrayCollection("addonServiceList");
                    addonService.Add(new JsonNumericValue("addonServiceUseType", 0));
                    addonService.Add(addonServiceList);
                    itemAddtionalInfo.Add(addonService);

                    itemAddtionalInfo.Add(new JsonNumericValue("goodsStatus", 1));


                    JsonObjectCollection addtionalInfo = new JsonObjectCollection("addtionalInfo");

                    JsonObjectCollection sellerDiscount = new JsonObjectCollection("sellerDiscount");
                    sellerDiscount.Add(new JsonBooleanValue("isUse", false));
                    addtionalInfo.Add(sellerDiscount);

                    JsonObjectCollection siteDiscount = new JsonObjectCollection("siteDiscount");
                    siteDiscount.Add(new JsonBooleanValue("gmkt", true));
                    siteDiscount.Add(new JsonBooleanValue("iac", true));
                    addtionalInfo.Add(siteDiscount);

                    JsonObjectCollection pcs = new JsonObjectCollection("pcs");
                    pcs.Add(new JsonBooleanValue("isUse", true));
                    pcs.Add(new JsonBooleanValue("isUseIacPcsCoupon", false));
                    pcs.Add(new JsonBooleanValue("isUseGmkPcsCoupon", false));
                    addtionalInfo.Add(pcs);

                    JsonObjectCollection overseaSales = new JsonObjectCollection("overseaSales");
                    overseaSales.Add(new JsonBooleanValue("isAgree", false));
                    addtionalInfo.Add(overseaSales);

                    JsonObjectCollection obj_total = new JsonObjectCollection();
                    obj_total.Add(isSell);
                    obj_total.Add(itemBasicInfo);
                    obj_total.Add(itemAddtionalInfo);
                    obj_total.Add(addtionalInfo);

                    string response = "";
                    using (WebClient client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        client.Headers.Add("Content-Type", "application/json");
                        client.Headers.Add("Authorization", Authorization);
                        response = client.UploadString("https://sa2.esmplus.com/item/v1/goods/"+dr["GOODSNO"].ToString()+"?isSync=true","PUT", obj_total.ToString());

                    }
                    JsonTextParser parser = new JsonTextParser();
                    JsonObjectCollection jc = (JsonObjectCollection)parser.Parse(response);

                    if (response.ToString().IndexOf("ResultCode") > -1)
                    {
                        InsertError(dr, jc["Message"].GetValue().ToString());
                    }else
                    {
                        JsonObjectCollection siteDetail = (JsonObjectCollection)jc["siteDetail"];
                        JsonObjectCollection gmkt = (JsonObjectCollection)siteDetail["gmkt"];
                        JsonObjectCollection iac = (JsonObjectCollection)siteDetail["iac"];

                        string gmkt_SiteGoodsComment = gmkt["SiteGoodsComment"].GetValue().ToString();
                        string iac_SiteGoodsComment = iac["SiteGoodsComment"].GetValue().ToString();
                        string gmkt_SiteGoodsNo = "";
                        string iac_SiteGoodsNo = "";
                        string goodsNo = "";

                        if (gmkt_SiteGoodsComment == "Success")
                        {
                            gmkt_SiteGoodsNo = gmkt["SiteGoodsNo"].GetValue().ToString();
                        }

                        if (iac_SiteGoodsComment == "Success")
                        {
                            iac_SiteGoodsNo = iac["SiteGoodsNo"].GetValue().ToString();
                        }

                        if (gmkt_SiteGoodsComment == "Success" || iac_SiteGoodsComment == "Success")
                        {
                            goodsNo = jc["goodsNo"].GetValue().ToString();
                            UptGoods(dr, goodsNo);
                        }

                        if (gmkt_SiteGoodsComment != "Success")
                        {

                        }
                        if (iac_SiteGoodsComment != "Success")
                        {

                        }
                    }

                  
                }
                catch(Exception ex)
                {
                    InsertError(dr, ex.Message.ToString());
                }
               
            }

            public void RegGoods(string Authorization, DataRow dr)
            {
                string ErrMsg = "";
                try
                {
                    Console.WriteLine("=============  상품등록 : " + dr["name"].ToString() + "  =============");
                    JsonObjectCollection itemBasicInfo = new JsonObjectCollection("itemBasicInfo");
                    JsonObjectCollection goodsName = new JsonObjectCollection("goodsName");
                    goodsName.Add(new JsonStringValue("kor", dr["name"].ToString()));
                    itemBasicInfo.Add(goodsName);

                    JsonObjectCollection category = new JsonObjectCollection("category");
                    JsonArrayCollection arr_site = new JsonArrayCollection("site");
                    JsonArrayCollection arr_esm = new JsonArrayCollection("esm");

                    JsonObjectCollection siteCode_1 = new JsonObjectCollection();
                    siteCode_1.Add(new JsonNumericValue("siteType", 1)); //1.옥션 2.G마켓
                    siteCode_1.Add(new JsonStringValue("catCode", dr["CAT_AUCTION"].ToString()));
                    JsonObjectCollection siteCode_2 = new JsonObjectCollection();
                    siteCode_2.Add(new JsonNumericValue("siteType", 2)); //1.옥션 2.G마켓
                    siteCode_2.Add(new JsonStringValue("catCode", dr["CAT_GMARKET"].ToString()));

                    JsonObjectCollection esm_catCode = new JsonObjectCollection("esm");
                    esm_catCode.Add(new JsonStringValue("catCode", dr["CAT_ESM"].ToString()));

                    arr_site.Add(siteCode_1);
                    arr_site.Add(siteCode_2);
                    category.Add(arr_site);
                    category.Add(esm_catCode);
                    itemBasicInfo.Add(category);

                    JsonObjectCollection book = new JsonObjectCollection("book");
                    book.Add(new JsonBooleanValue("isUseIsbnCode", true));
                    book.Add(new JsonStringValue("isbnCode", dr["barcode"].ToString()));
                    book.Add(new JsonNumericValue("price", Convert.ToInt32(dr["price"].ToString())));
                    itemBasicInfo.Add(book);

                    JsonObjectCollection catalog = new JsonObjectCollection("catalog");
                    JsonObjectCollection brand = GetEsmBrandInfo(Authorization, dr["publisher_name"].ToString());

                    catalog.Add(new JsonStringValue("modelName", dr["publisher_name"].ToString()));

                    if (brand["brandNo"].GetValue().ToString() != "0")
                    {
                        catalog.Add(new JsonNumericValue("brandNo", Convert.ToInt32(brand["brandNo"].GetValue().ToString())));
                    }

                    catalog.Add(new JsonStringValue("barCode", dr["barcode"].ToString()));
                    itemBasicInfo.Add(catalog);

                    JsonObjectCollection itemAddtionalInfo = new JsonObjectCollection("itemAddtionalInfo");
                    JsonObjectCollection buyableQuantity = new JsonObjectCollection("buyableQuantity");
                    buyableQuantity.Add(new JsonNumericValue("type", 0));
                    buyableQuantity.Add(new JsonNumericValue("qty", 999));
                    itemAddtionalInfo.Add(buyableQuantity);

                    JsonObjectCollection price = new JsonObjectCollection("price");
                    price.Add(new JsonNumericValue("Gmkt", Convert.ToInt32(dr["sale_price"].ToString())));
                    price.Add(new JsonNumericValue("Iac", Convert.ToInt32(dr["sale_price"].ToString())));
                    itemAddtionalInfo.Add(price);

                    JsonObjectCollection stock = new JsonObjectCollection("stock");
                    stock.Add(new JsonNumericValue("Gmkt", 9999));
                    stock.Add(new JsonNumericValue("Iac", 9999));
                    itemAddtionalInfo.Add(stock);

                    JsonObjectCollection sellingPeriod = new JsonObjectCollection("sellingPeriod");
                    sellingPeriod.Add(new JsonNumericValue("Gmkt", 90));
                    sellingPeriod.Add(new JsonNumericValue("Iac", 90));
                    itemAddtionalInfo.Add(sellingPeriod);


                    itemAddtionalInfo.Add(new JsonStringValue("managedCode", dr["pk_id"].ToString()));

                    JsonObjectCollection recommendedOpts = new JsonObjectCollection("recommendedOpts");
                    recommendedOpts.Add(new JsonNumericValue("type", 0));
                    itemAddtionalInfo.Add(recommendedOpts);

                    JsonObjectCollection orderOpts = new JsonObjectCollection("orderOpts");
                    orderOpts.Add(new JsonNumericValue("type", 0));
                    itemAddtionalInfo.Add(orderOpts);

                    itemAddtionalInfo.Add(new JsonStringValue("manufacturedDate", dr["press_date"].ToString()));  //제조일 YYYY-MM-DD 형태로 입력

                    JsonObjectCollection shipping = new JsonObjectCollection("shipping");
                    shipping.Add(new JsonNumericValue("type", 1));
                    shipping.Add(new JsonNumericValue("companyNo", 10013));  //택배사코드  10001 대한통운 , 10007 한진,10008 롯데,10013 CJ

                    JsonObjectCollection policy = new JsonObjectCollection("policy");
                    policy.Add(new JsonNumericValue("placeNo", 42357));  //출하지번호
                    policy.Add(new JsonNumericValue("feeType", 1));

                    JsonObjectCollection bundle = new JsonObjectCollection("bundle");
                    //묶음배송비정책번호
                    if (Convert.ToInt32(dr["MARGIN_AMOUNT"].ToString()) >= 200)
                    {
                        bundle.Add(new JsonNumericValue("deliveryTmplId", 4066482));
                    }
                    else
                    {
                        int sale_price = Convert.ToInt32(dr["sale_price"].ToString());

                        if (sale_price < 10000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2557321));
                        }
                        else if (sale_price < 15000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2543515));
                        }
                        else if (sale_price >= 15000 && sale_price < 20000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 171526));
                        }
                        else if (sale_price >= 20000 && sale_price < 25000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 3239986));
                        }
                        else if (sale_price >= 25000 && sale_price < 30000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 3239985));
                        }
                        else if (sale_price >= 30000 && sale_price < 35000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 3984347));
                        }
                        else if (sale_price >= 35000 && sale_price < 40000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2544645));
                        }
                        else if (sale_price >= 40000 && sale_price < 45000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 42130208));
                        }
                        else if (sale_price >= 45000 && sale_price < 50000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2810772));
                        }
                        else if (sale_price >= 50000)
                        {
                            bundle.Add(new JsonNumericValue("deliveryTmplId", 2810764));
                        }

                    }
                    policy.Add(bundle);
                    shipping.Add(policy);

                    JsonObjectCollection returnAndExchange = new JsonObjectCollection("returnAndExchange");
                    returnAndExchange.Add(new JsonNumericValue("addrNo", 139719));
                    //returnAndExchange.Add(new JsonNumericValue("shippingCompany", 10013));
                    returnAndExchange.Add(new JsonNumericValue("fee", 2500));
                    shipping.Add(returnAndExchange);

                    JsonObjectCollection dispatchPolicyNo = new JsonObjectCollection("dispatchPolicyNo");
                    dispatchPolicyNo.Add(new JsonNumericValue("gmkt", 243245));
                    dispatchPolicyNo.Add(new JsonNumericValue("iac", 243244));
                    shipping.Add(dispatchPolicyNo);



                    JsonObjectCollection visitAndTake = new JsonObjectCollection("visitAndTake");
                    visitAndTake.Add(new JsonBooleanValue("isUse", false));
                    shipping.Add(visitAndTake);

                    JsonObjectCollection quickService = new JsonObjectCollection("quickService");
                    quickService.Add(new JsonBooleanValue("isUse", false));
                    shipping.Add(quickService);

                    shipping.Add(new JsonStringValue("backwoodsDeliveryYn", "Y"));

                    itemAddtionalInfo.Add(shipping);


                    JsonObjectCollection officialNotice = new JsonObjectCollection("officialNotice");  //고시
                    officialNotice.Add(new JsonNumericValue("officialNoticeNo", 26));
                    JsonArrayCollection arr_details = new JsonArrayCollection("details");
                    JsonObjectCollection details_1 = new JsonObjectCollection();
                    details_1.Add(new JsonStringValue("officialNoticeItemelementCode", "26-1"));
                    details_1.Add(new JsonStringValue("value", dr["name"].ToString()));
                    details_1.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_1);

                    JsonObjectCollection details_2 = new JsonObjectCollection();
                    details_2.Add(new JsonStringValue("officialNoticeItemelementCode", "26-2"));
                    details_2.Add(new JsonStringValue("value", dr["author"].ToString() + "/" + dr["publisher_name"].ToString()));
                    details_2.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_2);


                    JsonObjectCollection details_3 = new JsonObjectCollection();
                    details_3.Add(new JsonStringValue("officialNoticeItemelementCode", "26-3"));
                    details_3.Add(new JsonStringValue("value", dr["size"].ToString() != "" ? dr["size"].ToString() : "상품상세참조"));
                    details_3.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_3);


                    JsonObjectCollection details_4 = new JsonObjectCollection();
                    details_4.Add(new JsonStringValue("officialNoticeItemelementCode", "26-4"));
                    details_4.Add(new JsonStringValue("value", dr["page"].ToString() != "" ? dr["page"].ToString() : "상품상세참조"));
                    details_4.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_4);

                    JsonObjectCollection details_5 = new JsonObjectCollection();
                    details_5.Add(new JsonStringValue("officialNoticeItemelementCode", "26-5"));
                    details_5.Add(new JsonStringValue("value", "상품상세참조"));
                    details_5.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_5);

                    JsonObjectCollection details_6 = new JsonObjectCollection();
                    details_6.Add(new JsonStringValue("officialNoticeItemelementCode", "26-6"));
                    details_6.Add(new JsonStringValue("value", dr["press_date"].ToString() != "" ? dr["press_date"].ToString() : "상품상세참조"));
                    details_6.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_6);


                    JsonObjectCollection details_7 = new JsonObjectCollection();
                    details_7.Add(new JsonStringValue("officialNoticeItemelementCode", "26-7"));
                    details_7.Add(new JsonStringValue("value", "상품상세참조"));
                    details_7.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_7);


                    JsonObjectCollection details_8 = new JsonObjectCollection();
                    details_8.Add(new JsonStringValue("officialNoticeItemelementCode", "26-8"));
                    details_8.Add(new JsonStringValue("value", "2일"));
                    details_8.Add(new JsonBooleanValue("isExtraMark", false));
                    arr_details.Add(details_8);

                    officialNotice.Add(arr_details);
                    itemAddtionalInfo.Add(officialNotice);

                    itemAddtionalInfo.Add(new JsonBooleanValue("isAdultProduct", false));
                    itemAddtionalInfo.Add(new JsonBooleanValue("isYouthNotAvailable", false));
                    itemAddtionalInfo.Add(new JsonBooleanValue("isVatFree", dr["tax_type"].ToString() == "N" ? true : false));

                    if (dr["KIDSAUTH"].ToString() != "0")
                    {
                        JsonObjectCollection certInfo = new JsonObjectCollection("certInfo");
                        JsonObjectCollection safetyCerts = new JsonObjectCollection("safetyCerts");
                        JsonObjectCollection child = new JsonObjectCollection("child");
                        JsonArrayCollection arr_child = new JsonArrayCollection("details");
                        child.Add(arr_child);
                        child.Add(new JsonNumericValue("type", 2));


                        safetyCerts.Add(child);
                        certInfo.Add(safetyCerts);
                        itemAddtionalInfo.Add(certInfo);

                    }



                    JsonObjectCollection images = new JsonObjectCollection("images");
                    images.Add(new JsonStringValue("basicImgURL", dr["image_url"].ToString()));
                    //images.Add(new JsonStringValue("addtionalImg1URL", dr["image_url"].ToString()));
                    itemAddtionalInfo.Add(images);

                    JsonObjectCollection descriptions = new JsonObjectCollection("descriptions");
                    JsonObjectCollection kor = new JsonObjectCollection("kor");
                    kor.Add(new JsonNumericValue("type", 2));
                    kor.Add(new JsonStringValue("html", GetContent(dr)));
                    descriptions.Add(kor);
                    itemAddtionalInfo.Add(descriptions);


                    JsonObjectCollection addonService = new JsonObjectCollection("addonService");
                    JsonArrayCollection addonServiceList = new JsonArrayCollection("addonServiceList");
                    addonService.Add(new JsonNumericValue("addonServiceUseType", 0));
                    addonService.Add(addonServiceList);
                    itemAddtionalInfo.Add(addonService);

                    itemAddtionalInfo.Add(new JsonNumericValue("goodsStatus", 1));


                    JsonObjectCollection addtionalInfo = new JsonObjectCollection("addtionalInfo");

                    JsonObjectCollection sellerDiscount = new JsonObjectCollection("sellerDiscount");
                    sellerDiscount.Add(new JsonBooleanValue("isUse", false));
                    addtionalInfo.Add(sellerDiscount);

                    JsonObjectCollection siteDiscount = new JsonObjectCollection("siteDiscount");
                    siteDiscount.Add(new JsonBooleanValue("gmkt", true));
                    siteDiscount.Add(new JsonBooleanValue("iac", true));
                    addtionalInfo.Add(siteDiscount);

                    JsonObjectCollection pcs = new JsonObjectCollection("pcs");
                    pcs.Add(new JsonBooleanValue("isUse", true));
                    pcs.Add(new JsonBooleanValue("isUseIacPcsCoupon", false));
                    pcs.Add(new JsonBooleanValue("isUseGmkPcsCoupon", false));
                    addtionalInfo.Add(pcs);

                    JsonObjectCollection overseaSales = new JsonObjectCollection("overseaSales");
                    overseaSales.Add(new JsonBooleanValue("isAgree", false));
                    addtionalInfo.Add(overseaSales);

                    JsonObjectCollection obj_total = new JsonObjectCollection();
                    obj_total.Add(itemBasicInfo);
                    obj_total.Add(itemAddtionalInfo);
                    obj_total.Add(addtionalInfo);

                    string response = "";
                    using (WebClient client = new WebClient())
                    {
                        client.Encoding = Encoding.UTF8;
                        client.Headers.Add("Content-Type", "application/json");
                        client.Headers.Add("Authorization", Authorization);
                        response = client.UploadString("https://sa2.esmplus.com/item/v1/goods?isSync=true", obj_total.ToString());

                    }

                    JsonTextParser parser = new JsonTextParser();
                    JsonObjectCollection jc = (JsonObjectCollection)parser.Parse(response);

                    if (response.ToString().IndexOf("ResultCode") > -1)
                    {
                        InsertError(dr, jc["Message"].GetValue().ToString());
                    }
                    else
                    {
                        
                        JsonObjectCollection siteDetail = (JsonObjectCollection)jc["siteDetail"];
                        JsonObjectCollection gmkt = (JsonObjectCollection)siteDetail["gmkt"];
                        JsonObjectCollection iac = (JsonObjectCollection)siteDetail["iac"];

                        string gmkt_SiteGoodsComment = gmkt["SiteGoodsComment"].GetValue().ToString();
                        string iac_SiteGoodsComment = iac["SiteGoodsComment"].GetValue().ToString();
                        string gmkt_SiteGoodsNo = "";
                        string iac_SiteGoodsNo = "";
                        string goodsNo = "";

                        if (gmkt_SiteGoodsComment == "Success")
                        {
                            gmkt_SiteGoodsNo = gmkt["SiteGoodsNo"].GetValue().ToString();
                        }

                        if (iac_SiteGoodsComment == "Success")
                        {
                            iac_SiteGoodsNo = iac["SiteGoodsNo"].GetValue().ToString();
                        }

                        if (gmkt_SiteGoodsComment == "Success" || iac_SiteGoodsComment == "Success")
                        {
                            goodsNo = jc["goodsNo"].GetValue().ToString();
                            InsertGoods(dr, goodsNo, gmkt_SiteGoodsNo, iac_SiteGoodsNo);
                        }

                        if (gmkt_SiteGoodsComment != "Success")
                        {
                            InsertError(dr, iac_SiteGoodsComment);
                        }
                        if (iac_SiteGoodsComment != "Success")
                        {
                            InsertError(dr, iac_SiteGoodsComment);
                        }
                    }

               

                }
                catch(Exception ex)
                {
                    InsertError(dr, ex.Message.ToString());
                }
                
            }
            public void InsertGoods(DataRow dr, string GOODSNO,string gmkt_SiteGoodsNo,string iac_SiteGoodsNo)
            {
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";
                // 오라클 연결
                OracleConnection conn = new OracleConnection(_strConn);

                try
                {
                    conn.Open();

                    // 명령 객체 생성
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;

                    // SQL문 지정 및 INSERT 실행
                    cmd.CommandText = @"INSERT INTO EGOODS_ESM (GOODSNO, PK_ID, NEW_DATE, UPD_GB,EXPIRED_DATE,GMARKET_NO,AUCTION_NO) VALUES ('" + GOODSNO + "', '" + dr["pk_id"].ToString() + "',TO_CHAR(sysdate,'yyyymmddhh24miss'),'" + dr["PJ_GB"].ToString() + "',TO_CHAR(SYSDATE +89,'YYYY-MM-DD'),'"+ gmkt_SiteGoodsNo+"','"+ iac_SiteGoodsNo + "')";
                    cmd.ExecuteNonQuery();


                }
                catch (Exception ex)
                {

                }
                finally
                {
                    conn.Close();
                }
            }

            public void UptGoods(DataRow dr, string GOODSNO)
            {
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";
                // 오라클 연결
                OracleConnection conn = new OracleConnection(_strConn);

                try
                {
                    conn.Open();

                    // 명령 객체 생성
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;

                    // SQL문 지정 및 INSERT 실행
                    cmd.CommandText = @"UPDATE EGOODS_ESM SET UPD_MSG='상품 업데이트', UPD_DATE=TO_CHAR(sysdate,'yyyymmddhh24miss'), UPD_GB='" + dr["PJ_GB"].ToString() + "'  WHERE GOODSNO= '" + GOODSNO + "'";

                    cmd.ExecuteNonQuery();


                }
                catch (Exception ex)
                {

                }
                finally
                {
                    conn.Close();
                }


            }

            public string InitAuth()
            {
                string secretkey = "EiLVkldgxkati0P2X/A0xg==";

                JsonObjectCollection objHeader = new JsonObjectCollection();
                objHeader.Add(new JsonStringValue("alg", "HS256"));
                objHeader.Add(new JsonStringValue("typ", "JWT"));
                objHeader.Add(new JsonStringValue("kid", "jbooks"));

                JsonObjectCollection objPayload = new JsonObjectCollection();
                objPayload.Add(new JsonStringValue("iss", "http://www.jbookshop.co.kr"));
                objPayload.Add(new JsonStringValue("sub", "sell"));
                objPayload.Add(new JsonStringValue("aud", "sa.esmplus.com"));
                objPayload.Add(new JsonStringValue("iat", DateTime.UtcNow.Ticks.ToString()));
                objPayload.Add(new JsonStringValue("ssi", "A:ysfafa,G:jbooks"));
                //objPayload.Add(new JsonStringValue("ssi", "A:ysfafa,G:jbooks"));

                string Base64_header = Base64Encode(objHeader.ToString());
                string Base64_Payload = Base64Encode(objPayload.ToString());

                string HS256 = HmacSHA256(Base64_header + "." + Base64_Payload, secretkey);

                return "Bearer " + Base64_header + "." + Base64_Payload + "." + HS256;

            }
            public JsonObjectCollection GetEsmBrandInfo(string Authorization, string brand)
            {
                JsonObjectCollection rtn_obj = new JsonObjectCollection();

                try
                {
                    string response = "";
                    using (WebClient client = new WebClient())
                    {
                        
                        client.Encoding = Encoding.UTF8;
                        client.Headers.Add("Authorization", Authorization);
                        response = client.DownloadString("https://sa2.esmplus.com/item/v1/catalogs/brands/" + brand);

                    }
                    JsonTextParser parser = new JsonTextParser();
                    JsonObjectCollection jc = (JsonObjectCollection)parser.Parse(response);

                    Int32 brandNo = 0;
                    string brandName = "";
                    string makerNo = "";
                    string makerName = "";
                    //string productBrandNo = "";
                    //string productBrandName = "";

                    JsonArrayCollection arr = (JsonArrayCollection)jc["brands"];
                    foreach (JsonObjectCollection obj in arr)
                    {
                        if (brand == obj["brandName"].GetValue().ToString())
                        {
                            brandNo = Convert.ToInt32(obj["brandNo"].GetValue().ToString());
                            brandName = obj["brandName"].GetValue().ToString();
                            makerNo = obj["makerNo"].GetValue().ToString();
                            makerName = obj["makerName"].GetValue().ToString();

                            rtn_obj.Add(new JsonNumericValue("brandNo", brandNo));
                            rtn_obj.Add(new JsonStringValue("brandName", brandName));
                            rtn_obj.Add(new JsonStringValue("makerNo", makerNo));
                            rtn_obj.Add(new JsonStringValue("makerName", makerName));
                        }
                        else
                        {
                            rtn_obj.Add(new JsonNumericValue("brandNo", brandNo));
                            rtn_obj.Add(new JsonStringValue("brandName", brandName));
                            rtn_obj.Add(new JsonStringValue("makerNo", makerNo));
                            rtn_obj.Add(new JsonStringValue("makerName", makerName));
                        }

                    }
                }
                catch (Exception ex)
                {
                    rtn_obj.Add(new JsonNumericValue("brandNo", 0));
                    rtn_obj.Add(new JsonStringValue("brandName", ""));
                    rtn_obj.Add(new JsonStringValue("makerNo", ""));
                    rtn_obj.Add(new JsonStringValue("makerName", ""));
                    return rtn_obj;
                }

                return rtn_obj;

            }

            public void regCategory(string Authorization, DataRow dr)
            {


                string response = "";
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add("Authorization", Authorization);

                    //URL만 호출 / Request Body 없음
                    //[GET] https://sa2.esmplus.com/item/v1/categories/site-cats/100000002    
                    //전체 카테고리 조회
                    //[GET] https://sa2.esmplus.com/item/v1/categories/site-cats

                    //{"catCode":"100000028","catName":"도서음반/e교육","isLeaf":false} https://sa2.esmplus.com/item/v1/categories/site-cats/100000028 지마켓
                    //{"catCode":"36000000","catName":"도서/교육/음반","isLeaf":false} https://sa2.esmplus.com/item/v1/categories/site-cats/36000000 옥션
                    response = client.DownloadString("https://sa2.esmplus.com/item/v1/categories/site-cats/36000000");

                }



                JsonTextParser parser = new JsonTextParser();
                JsonObjectCollection jc = (JsonObjectCollection)parser.Parse(response);

                string catCode = jc["catCode"].GetValue().ToString();
                string FirstcatName = jc["catName"].GetValue().ToString();
                string isLeaf = jc["isLeaf"].GetValue().ToString();
                string catName = jc["catName"].GetValue().ToString();

                if (isLeaf == "True")
                {
                    RegCat("auction", catCode, FirstcatName + catName);
                }

                JsonArrayCollection arr = (JsonArrayCollection)jc["subCats"];
                foreach (JsonObjectCollection obj in arr)
                {
                    isLeaf = obj["isLeaf"].GetValue().ToString();
                    catCode = obj["catCode"].GetValue().ToString();
                    catName = obj["catName"].GetValue().ToString();
                    if (isLeaf == "True")
                    {
                        RegCat("auction", catCode, FirstcatName + ">" + catName);
                    }
                    else
                    {
                        RecursiveFactorial("auction", catCode, FirstcatName + ">" + catName, isLeaf, Authorization);
                    }
                }

            }

            public void regEsmCategory(string Authorization)
            {

                string response = "";
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add("Authorization", Authorization);
                    //var response = client.DownloadString("https://sa2.esmplus.com/item/v1/categories/site-cats/100000028");
                    response = client.DownloadString("https://sa2.esmplus.com/item/v1/categories/sd-cats/00170000000000000000");

                }



                JsonTextParser parser = new JsonTextParser();
                JsonObjectCollection jc = (JsonObjectCollection)parser.Parse(response);

                string catCode = "";
                string FirstcatName = "";
                string isLeaf = "";
                string catName = "";



                JsonArrayCollection arr = (JsonArrayCollection)jc["sdCategoryTree"];
                foreach (JsonObjectCollection obj in arr)
                {
                    catCode = obj["SDCategoryCode"].GetValue().ToString();
                    catName = obj["SDCategoryName"].GetValue().ToString();
                    isLeaf = obj["IsLeafCategory"].GetValue().ToString();
                    if (isLeaf == "True")
                    {
                        RegCat("esm", catCode, "도서" + " > " + catName);
                    }
                    else
                    {
                        RecursiveFactorialEsm("esm", catCode, "도서" + " > " + catName, isLeaf, Authorization);
                    }
                }

            }
            public void RecursiveFactorialEsm(string channel, string code, string name, string leaf, string Authorization)
            {
                string response = "";
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add("Authorization", Authorization);
                    response = client.DownloadString("https://sa2.esmplus.com/item/v1/categories/sd-cats/" + code);

                }

                JsonTextParser parser = new JsonTextParser();
                JsonObjectCollection jc = (JsonObjectCollection)parser.Parse(response);

                string catCode = "";
                string catName = "";
                string isLeaf = "";

                JsonArrayCollection arr = (JsonArrayCollection)jc["sdCategoryTree"];


                foreach (JsonObjectCollection obj in arr)
                {
                    isLeaf = obj["IsLeafCategory"].GetValue().ToString();
                    catCode = obj["SDCategoryCode"].GetValue().ToString();
                    catName = obj["SDCategoryName"].GetValue().ToString();
                    if (isLeaf == "True")
                    {
                        RegCat(channel, catCode, name + ">" + catName);
                    }
                    else
                    {
                        RecursiveFactorialEsm("esm", catCode, name + ">" + catName, isLeaf, Authorization);
                    }
                }


            }


            public void RecursiveFactorial(string channel, string code, string name, string leaf, string Authorization)
            {
                string response = "";
                using (WebClient client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    client.Headers.Add("Authorization", Authorization);
                    response = client.DownloadString("https://sa2.esmplus.com/item/v1/categories/site-cats/" + code);

                }

                JsonTextParser parser = new JsonTextParser();
                JsonObjectCollection jc = (JsonObjectCollection)parser.Parse(response);

                string catCode = jc["catCode"].GetValue().ToString();
                string catName = jc["catName"].GetValue().ToString();
                string isLeaf = jc["isLeaf"].GetValue().ToString();

                JsonArrayCollection arr = (JsonArrayCollection)jc["subCats"];
                if (leaf == "True")
                {
                    RegCat(channel, code + ">" + catCode, catName);
                    return;
                }

                foreach (JsonObjectCollection obj in arr)
                {
                    isLeaf = obj["isLeaf"].GetValue().ToString();
                    catCode = obj["catCode"].GetValue().ToString();
                    catName = obj["catName"].GetValue().ToString();
                    if (isLeaf == "True")
                    {
                        RegCat(channel, catCode, name + ">" + catName);
                    }
                    else
                    {
                        RecursiveFactorial("auction", catCode, name + ">" + catName, isLeaf, Authorization);
                    }
                }


            }


            public void RegCat(string channel, string code, string name)
            {
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";
                // 오라클 연결
                OracleConnection conn = new OracleConnection(_strConn);

                try
                {
                    conn.Open();

                    // 명령 객체 생성
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;

                    // SQL문 지정 및 INSERT 실행
                    cmd.CommandText = @"INSERT INTO IF_ESM_CATEGORY (CHANNEL, CATCCODE,CATNAME) VALUES ('" + channel + "', '" + code + "','" + name + "')";
                    cmd.ExecuteNonQuery();


                }
                catch (Exception ex)
                {

                }
                finally
                {
                    conn.Close();
                }
            }
            private static string HmacSHA256(string message, string secret)
            {

                using (HMACSHA256 hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secret)))
                {
                    return Convert.ToBase64String((hmac.ComputeHash(Encoding.ASCII.GetBytes(message))));
                }
            }

            public static string Base64Encode(string data)
            {
                try
                {
                    byte[] encData_byte = new byte[data.Length];

                    encData_byte = System.Text.Encoding.UTF8.GetBytes(data);

                    string encodedData = Convert.ToBase64String(encData_byte);
                    return encodedData;
                }
                catch (Exception e)
                {
                    throw new Exception("Error in Base64Encode: " + e.Message);

                }

            }
            public string GetContent(DataRow dr)
            {
                string Str_Contents = "<div style=\"text-align: left;\">";

                if (!string.IsNullOrEmpty(dr["kc_certification"].ToString()))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<p style=\"width:50%; padding-top:4em;\"><img src=\"http://www.jbookshop.co.kr/wemake/image/kc_3.png\" style=\"width:20em;\"></p>");
                    sb.Append("<p style=\"padding-top: 2em; padding-left:2em; padding-bottom:3em;line-height:2.0em; font-size: 1.2em;\">");
                    sb.Append("인증번호: " + dr["kc_certification"].ToString() + " KC마크는 해당 제품이 공통 안전기준에 적합하였음을 의미합니다.</p><br>");

                    Str_Contents = Str_Contents + sb.ToString();
                }
                //책소개
                if (dr["intro"].ToString() != "")
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append("<h4 style=\"margin: 28px 0px 0px; padding: 0px;font-size:16px;line-height:18px;height:26px;padding-top:4px;color:#000;font-family:'Malgun Gothic','Apple SD Gothic Neo','Dotum','Sans-Serif';\">책소개</h4>");
                    sb.Append("<div style=\"color: rgb(128, 128, 128); line-height: 1.5; margin-top: 10px;font-size:16px;\">");
                    sb.Append(StripHtml(dr["intro"].ToString().Replace("\r\n", "<br>"), "b", "br") + "</div>");


                    //if (!string.IsNullOrEmpty(img))
                    //{
                    //    if (img.IndexOf("kyobo") < 0)
                    //        sb.Append("<br>" + img);
                    //}
                    Str_Contents = Str_Contents + sb.ToString();
                }


                //목차
                if (!string.IsNullOrEmpty(dr["content"].ToString()))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<h4 style=\"margin: 28px 0px 0px; padding: 0px;font-size:16px;line-height:18px;height:26px;padding-top:4px;color:#000;font-family:'Malgun Gothic','Apple SD Gothic Neo','Dotum','Sans-Serif';\">목차</h4>");

                    sb.Append("<div style=\"color: rgb(128, 128, 128); line-height: 1.5; margin-top: 10px;;font-size:16px;\">");

                    string mocha = "곧 업데이트 될 예정입니다.<br>잠시만 기다려주세요…<br>자세한 상담 및 문의는 고객센터로 연락주세요.";
                    if (string.IsNullOrEmpty(dr["content"].ToString()))
                    {
                        sb.Append(mocha + "</div>");
                    }
                    else
                    {
                        sb.Append(StripHtml(dr["content"].ToString().Replace("\r\n", "<br>"), "b", "br") + "</div>");
                    }

                    Str_Contents = Str_Contents + sb.ToString();
                }




                //저자
                //if (!string.IsNullOrEmpty(dr["author"].ToString()))
                //{
                //    StringBuilder sb = new StringBuilder();


                //    sb.Append("<h4 style=\"margin: 28px 0px 0px; padding: 0px;font-size:16px;line-height:18px;height:26px;padding-top:4px;color:#000;font-family:'Malgun Gothic','Apple SD Gothic Neo','Dotum','Sans-Serif';\">작가</h4>");
                //    sb.Append("<div style=\"color: rgb(128, 128, 128); line-height: 1.5; margin-top: 10px;;font-size:16px;\">");
                //    sb.Append(" " + dr["author"].ToString().Replace("\r\n", "<br>") + "<BR></div>");

                //    Str_Contents = Str_Contents + sb.ToString();
                //}


                ////출판사 리뷰
                //if (!string.IsNullOrEmpty(Pub_Review))
                //{
                //    StringBuilder sb = new StringBuilder();


                //    sb.Append("<h4 style=\"margin: 28px 0px 0px; padding: 0px;font-size:16px;line-height:18px;height:26px;padding-top:4px;color:#000;font-family:'Malgun Gothic','Apple SD Gothic Neo','Dotum','Sans-Serif';\">출판사리뷰</h4>");

                //    sb.Append("<div style=\"color: rgb(128, 128, 128); line-height: 1 ㅈ.5; margin-top: 10px;;font-size:16px;\">");
                //    sb.Append(Pub_Review.Replace("\r\n", "<br>") + "</div>");
                //    Str_Contents = Str_Contents + sb.ToString();
                //}


                Str_Contents = Str_Contents + "</div>";


                return Str_Contents;
            }
            private string StripHtml(string sHtml, params string[] tagList)
            {
                StringBuilder sPattern = new StringBuilder();
                sPattern.Append("<(/)?");
                foreach (string tag in tagList)
                {
                    sPattern.AppendFormat("(?!/?{0})", tag);
                }
                sPattern.Append("([a-zA-Z\\d]*)(\\s[a-zA-Z\\d]*=[^>]*)?(\\s)*(/)?[^>]*>");
                return Regex.Replace(sHtml, sPattern.ToString(), string.Empty, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }


            public DataTable SetBookr(DataTable dt)
            {
                DataTable dtSetBook = new DataTable();
                dtSetBook.Columns.Add("pk_id", typeof(string));
                dtSetBook.Columns.Add("GOODSNO", typeof(string));
                dtSetBook.Columns.Add("barcode", typeof(string));
                dtSetBook.Columns.Add("CAT_GMARKET", typeof(string));
                dtSetBook.Columns.Add("CAT_AUCTION", typeof(string));
                dtSetBook.Columns.Add("CAT_ESM", typeof(string));
                dtSetBook.Columns.Add("name", typeof(string));
                dtSetBook.Columns.Add("author", typeof(string));
                dtSetBook.Columns.Add("price", typeof(string));
                dtSetBook.Columns.Add("sale_price", typeof(string));
                dtSetBook.Columns.Add("image_name", typeof(string));
                dtSetBook.Columns.Add("aladin_img", typeof(string));
                dtSetBook.Columns.Add("MARGIN_AMOUNT", typeof(string));
                dtSetBook.Columns.Add("intro", typeof(string));
                dtSetBook.Columns.Add("content", typeof(string));
                dtSetBook.Columns.Add("press_date", typeof(string));
                dtSetBook.Columns.Add("size", typeof(string));
                dtSetBook.Columns.Add("page", typeof(string));
                dtSetBook.Columns.Add("discount", typeof(string));
                dtSetBook.Columns.Add("kc_certification", typeof(string));
                dtSetBook.Columns.Add("ship_cost", typeof(string));
                dtSetBook.Columns.Add("content_type", typeof(string));
                dtSetBook.Columns.Add("PJ_GB", typeof(string));
                dtSetBook.Columns.Add("publisher_code", typeof(string));
                dtSetBook.Columns.Add("publisher_name", typeof(string));
                dtSetBook.Columns.Add("tax_type", typeof(string));
                dtSetBook.Columns.Add("image_url", typeof(string));
                dtSetBook.Columns.Add("KIDSAUTH", typeof(string));
                

                DataRow dr = dtSetBook.NewRow();

                dr["pk_id"] = dt.Rows[0]["PK_ID"].ToString();
                dr["GOODSNO"] = dt.Rows[0]["GOODSNO"].ToString();

                dr["barcode"] = dt.Rows[0]["BK_CD"].ToString();
                dr["CAT_GMARKET"] = dt.Rows[0]["CAT_GMARKET"].ToString();
                dr["CAT_AUCTION"] = dt.Rows[0]["CAT_AUCTION"].ToString();
                dr["CAT_ESM"] = dt.Rows[0]["CAT_ESM"].ToString();
                dr["name"] = dt.Rows[0]["BK_NM"].ToString();
                dr["author"] = dt.Rows[0]["AUTHOR"].ToString();
                dr["price"] = dt.Rows[0]["PRICE"].ToString();
                dr["sale_price"] = dt.Rows[0]["DIS_PRICE"].ToString();
                dr["image_name"] = dt.Rows[0]["MIMG_FNM"].ToString();
                dr["aladin_img"] = dt.Rows[0]["IMG_URL"].ToString();
                dr["MARGIN_AMOUNT"] = dt.Rows[0]["MARGIN_AMOUNT"].ToString();
                dr["intro"] = dt.Rows[0]["BK_INTRO"].ToString();
                dr["content"] = dt.Rows[0]["BK_CONT"].ToString();
                dr["press_date"] = Convert.ToDateTime(dt.Rows[0]["PRESS_DT"].ToString()).ToString("yyyy-MM-dd");
                dr["size"] = dt.Rows[0]["BK_SIZE"].ToString().Replace("*","x");
                dr["page"] = dt.Rows[0]["BK_PAGE"].ToString();
                dr["discount"] = dt.Rows[0]["DIS_RATE"].ToString();
                dr["kc_certification"] = dt.Rows[0]["AWARD_HST"].ToString();
                dr["ship_cost"] = dt.Rows[0]["SHIP_COST"].ToString();
                dr["content_type"] = dt.Rows[0]["MEDIA_FILE"].ToString();
                dr["PJ_GB"] = dt.Rows[0]["PJ_GB"].ToString();
                dr["publisher_code"] = dt.Rows[0]["PUB_CD"].ToString();
                dr["publisher_name"] = dt.Rows[0]["PUB_NM"].ToString();
                dr["tax_type"] = dt.Rows[0]["TAX_TYPE"].ToString() == "FREE" ? "N" : "Y";
                dr["publisher_name"] = dt.Rows[0]["PUB_NM"].ToString();
                dr["image_url"] = dr["image_name"].ToString() != "" ? "http://www.jbookshop.co.kr/BOOK_Image/Big/" + dr["image_name"].ToString() + ".jpg" : "no_img";
                dr["KIDSAUTH"] = dt.Rows[0]["KIDSAUTH"].ToString();  


                dtSetBook.Rows.Add(dr.ItemArray);

                return dtSetBook;
            }
            public DataTable GetGoodsDetailList(string pk_id)
            {

                DataTable dtGoods = new DataTable();
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";

                OracleConnection OraConn = new OracleConnection(_strConn);

                //sql에 저장된 데이터베이스 정보로 연결

                OraConn.Open();//디비 오픈

                OracleDataAdapter oda = new OracleDataAdapter();//어댑터 생성자


                //oda.SelectCommand = new OracleCommand("select * from VX_IFBIBLIO_ONLINE WHERE isbn13='9788956056722'", OraConn);
                oda.SelectCommand = new OracleCommand(@"SELECT  A.PK_ID, A.BK_CD, A.BK_NM, A.PUB_CD, A.PUB_NM, A.PRICE, A.DIS_PRICE, A.MIMG_FNM, A.TAX_TYPE, f_mj_amt(A.BK_CD,15) MARGIN_AMOUNT,
                A.BK_INTRO, A.BK_CONT, A.AUTHOR, A.BK_SIZE,TO_DATE(A.PRESS_DT,'YYYY-MM-DD') AS PRESS_DT, A.BK_PAGE, A.DIS_RATE, A.AWARD_HST, A.PJ_GB, A.SHIP_COST, A.MEDIA_FILE,            
                 (SELECT MAX(B.GMARKET) FROM CATE_ESM B WHERE A.BK_PART=B.BK_PART) CAT_GMARKET,
                (SELECT MAX(B.AUCTION) FROM CATE_ESM B WHERE A.BK_PART=B.BK_PART) CAT_AUCTION,
                (SELECT MAX(B.ESM) FROM CATE_ESM B WHERE A.BK_PART=B.BK_PART) CAT_ESM,
                'http://image.jbookshop.co.kr/Big/' || mimg_fnm || '.jpg' IMG_URL,
                (SELECT GOODSNO FROM EGOODS_ESM WHERE PK_ID = A.PK_ID) AS GOODSNO,
                F_CATE_AUCTIONKIDS(A.BK_PART) AS KIDSAUTH                
            FROM EBOOKCD A 
            WHERE A.PK_ID= '" + pk_id + "'", OraConn);

                oda.Fill(dtGoods);

                OraConn.Close();

                return dtGoods;
            }

            public void InsertError(DataRow dr, string msg)
            {
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";
                // 오라클 연결
                OracleConnection conn = new OracleConnection(_strConn);

                try
                {
                    conn.Open();

                    // 명령 객체 생성
                    OracleCommand cmd = new OracleCommand();
                    cmd.Connection = conn;

                    // SQL문 지정 및 INSERT 실행
                    cmd.CommandText = @"INSERT INTO ELOG_ESM(LOG_DATE, PK_ID, LOG_DESC, SELLER_ID) VALUES (TO_CHAR(sysdate,'yyyymmddhh24miss'),'" + dr["pk_id"].ToString() + "','" + msg + "','bombom')";
                    cmd.ExecuteNonQuery();


                }
                catch (Exception ex)
                {

                }
                finally
                {
                    conn.Close();
                }
            }

            public DataTable GetCntGoodsList(string pk_id)
            {

                DataTable dtGoods = new DataTable();
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";

                OracleConnection OraConn = new OracleConnection(_strConn);

                //sql에 저장된 데이터베이스 정보로 연결

                OraConn.Open();//디비 오픈

                OracleDataAdapter oda = new OracleDataAdapter();//어댑터 생성자


                //oda.SelectCommand = new OracleCommand("select * from VX_IFBIBLIO_ONLINE WHERE isbn13='9788956056722'", OraConn);
                oda.SelectCommand = new OracleCommand(@" select pk_id from EGOODS_ESM where pk_id = '" + pk_id + "'", OraConn);

                oda.Fill(dtGoods);

                OraConn.Close();

                return dtGoods;
            }

            public DataTable GetGoodsList()
            {
                DataTable Dt_Main = new DataTable();
                string _strConn = "Data Source=jbdb;User ID = webman; Password = webadm3985";

                OracleConnection OraConn = new OracleConnection(_strConn);

                //sql에 저장된 데이터베이스 정보로 연결

                OraConn.Open();//디비 오픈

                OracleDataAdapter oda = new OracleDataAdapter();//어댑터 생성자
                                                                //and revise_dt is null 
                oda.SelectCommand = new OracleCommand(@"
                                                        select 
                                                          rnum, 
                                                          pk_id, 
                                                          new_date, 
                                                          bk_nm 
                                                        from 
                                                          (
                                                            select 
                                                              rownum rnum, 
                                                              pk_id, 
                                                              new_date, 
                                                              bk_nm 
                                                            from 
                                                              (
                                                                select 
                                                                  a.pk_id, 
                                                                  to_date(
                                                                    substr(a.new_date, 1, 8), 
                                                                    'yyyymmdd'
                                                                  ) new_date, 
                                                                  a.bk_nm 
                                                                from 
                                                                  ebookcd a 
                                                                where 
                                                                  a.pj_gb = 'C' 
                                                                  and a.pk_gb <> 'N' 
                                                                  and a.mimg_fnm is not null 
                                                                  and (new_date > '20221123000001' OR  UPD_DATE > TO_CHAR(sysdate-2,'yyyymmddhh24miss')   )            
                                                                  and not exists (
                                                                    select 
                                                                      1 
                                                                    from 
                                                                      EGOODS_ESM b 
                                                                    where 
                                                                      b.pk_id = a.pk_id
                                                                  )                                                                                                                                                                                                                                                              
                                                                order by 
                                                                  new_date desc
                                                              )
                                                          )", OraConn);

                oda.Fill(Dt_Main);

                OraConn.Close();

                return Dt_Main;
            }
        }
    }
}
