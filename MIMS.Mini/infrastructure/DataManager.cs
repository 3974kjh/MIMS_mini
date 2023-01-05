using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MIMS.Mini.Foundation;
using MIMS.Mini.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using static MIMS.Mini.Defines.MIMSMiniMessage;

namespace MIMS.Mini.Infrastructure
{
    public class DataManager
    {
        IMongoDatabase _database;
        private ObjectId imageID;
        private ObjectId tImageID;

        public bool Init()                                                                         // database 구축, 연결
        {
            var client = new MongoClient("mongodb://localhost:27017");

            _database = client.GetDatabase("MIMSMiniDB");

            return true;
        }

        public long GetNewPatientNumber()
        {
            if (null == _database) 
                return -1;

            var collection = _database.GetCollection<BsonDocument>("patientinfo");

            if (null == collection)
                return -1;

            var lastPatientInfo =  collection.Find(new BsonDocument()).Sort(new BsonDocument("PatientNumber", -1)).FirstOrDefault();

            if (null == lastPatientInfo)
                return 1;

            if (false == lastPatientInfo.TryGetValue("PatientNumber", out BsonValue lastPatientNumber))
                return -1;

            return (long)lastPatientNumber + 1;
        }

        public void CreateFolder() // CreateFolder
        {
            string folderPath = AppDomain.CurrentDomain.BaseDirectory + @"\PatientThumbnailImage";

            if (false == Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        //-----------------------------------------------------<환자 이미지 정보 DB>------------------------------------------------

        private static void DownloadFile(GridFSBucket fs, ObjectId id, string downloadImagePath)
        {
            using (Stream image = new FileStream(downloadImagePath, FileMode.Create))
            {
                var t = fs.DownloadToStreamAsync(id, image);
 
                Task.WaitAll(t);
            }     
        }

        private static ObjectId UploadOriginalFile(GridFSBucket fs, string filepath)                                 // 원본 image upload
        {
            using (var s = File.OpenRead(filepath))
            {
                var t = Task.Run<ObjectId>(() =>
                {
                    return
                    fs.UploadFromStreamAsync(Path.GetFileName(filepath), s);
                });

                return t.Result;
            }
        }

        private static ObjectId UploadThumbnailFile(GridFSBucket fs, string filepath)                                 // 썸네일 image upload
        {
            Image image = Image.FromFile(filepath);
            Image thumb = image.GetThumbnailImage(100, 115, () => false, IntPtr.Zero);
            string thumbnailFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "PatientThumbnailImage", Path.GetFileName(filepath) + ".png");

            thumb.Save(thumbnailFilePath);
            image.Dispose();
            thumb.Dispose();

            using (var s = File.OpenRead(thumbnailFilePath))
            {
                var t = Task.Run<ObjectId>(() =>
                {
                    return
                    fs.UploadFromStreamAsync(Path.GetFileName(thumbnailFilePath), s);
                });

                return t.Result;
            }
        }

        public bool GetImageID(string filePath, out ObjectId imageID, out ObjectId tImageID)                                     // 이미지 파일 저장하는 함수	
        {
            bool success = false;
            imageID = default;
            tImageID = default;
            
            try
            {
                var fs = new GridFSBucket(_database);

                imageID = UploadOriginalFile(fs, filePath);
                tImageID = UploadThumbnailFile(fs, filePath);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[GetImageID] " + e.Message);
            }

            return success;
        }

        public bool DownloadImage(ObjectId ImageID, string imageUniqueName)                                     // 이미지 파일 다운로드하는 함수	
        {
            bool success = false;

            try
            {
                var fs = new GridFSBucket(_database);
                string downloadImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "PatientThumbnailImage", imageUniqueName + ".png");

                if (false == File.Exists(downloadImagePath))
                    DownloadFile(fs, ImageID, downloadImagePath);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DownloadImage] " + e.Message);
            }

            return success;
        }


        public bool InsertImageInfo(string filePath, int imageCategory, PatientInfoModel _patientInfo)          // 이미지 정보 등록하는 함수      
        {
            bool success = false;

            try
            {
                if (null == _patientInfo || null == _database)
                    throw new ApplicationException("imageInfo or database is null");

                if (false == GetImageID(filePath, out imageID, out tImageID))
                    return success;

                var imageModel = new ImageInfoModel();

                imageModel.ImageID = imageID;
                imageModel.TimageID = tImageID;
                imageModel.ImageDate = DateTime.Now;
                imageModel.PatientName = _patientInfo.PatientName;
                imageModel.PatientNumber = _patientInfo.PatientNumber;
                imageModel.ImageCategory = imageCategory;
                imageModel.ImageUniqueName = imageModel.PatientNumber.ToString() + imageModel.ImageDate.ToString("yyyyMMddTHHmmss") + imageModel.ImageCategory.ToString();
                imageModel.TimageUniqueName = "T" + imageModel.PatientNumber.ToString() + imageModel.ImageDate.ToString("yyyyMMddTHHmmss") + imageModel.ImageCategory.ToString();

                var collection = _database.GetCollection<BsonDocument>("patientimageinfo");
                var document = new BsonDocument()
                {
                    {"ImageID", imageModel.ImageID},
                    {"TimageID", imageModel.TimageID},
                    {"ImageDate", imageModel.ImageDate},
                    {"PatientName", imageModel.PatientName},
                    {"PatientNumber", imageModel.PatientNumber},
                    {"ImageCategory", imageModel.ImageCategory},
                    {"ImageUniqueName", imageModel.ImageUniqueName},
                    {"TimageUniqueName", imageModel.TimageUniqueName},
                };
                collection.InsertOne(document);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[InsertImageInfo] " + e.Message);
            }

            return success;
        }

        public List<ImageInfoModel> GetTimageInfoList(long patientNumber)                             // DB에서 썸네일 영상 다운받고 썸네일이미지 리스트 모두 불러오는 함수
        {
            List<ImageInfoModel> patientImageList = new List<ImageInfoModel>();

            try
            {
                if (null == _database)
                    throw new ApplicationException("database is null");

                var collection = _database.GetCollection<BsonDocument>("patientimageinfo");
                var filter = Builders<BsonDocument>.Filter.Eq("PatientNumber", patientNumber);
                var cursor = collection.Find(filter).ToCursor();

                foreach (var document in cursor.ToEnumerable())
                {
                    DownloadImage(document["TimageID"].AsObjectId, document["TimageUniqueName"].ToString());

                    ImageInfoModel patientimage = new ImageInfoModel();

                    patientimage.ImageID = document["ImageID"].AsObjectId;
                    patientimage.TimageID = document["TimageID"].AsObjectId;
                    patientimage.PatientNumber = document["PatientNumber"].ToInt64();
                    patientimage.ImageDate = document["ImageDate"].ToLocalTime();
                    patientimage.TImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "PatientThumbnailImage", document["TimageUniqueName"].ToString() + ".png");
                    patientimage.ImageCategory = document["ImageCategory"].ToInt32();
                    patientimage.ImageUniqueName = document["ImageUniqueName"].ToString();
                    patientimage.TimageUniqueName = document["TimageUniqueName"].ToString();

                    patientImageList.Add(patientimage);
                }

                patientImageList.Reverse(); // 최근 자료부터 불러오기
            }
            catch(Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[GetTimageInfoList] " + e.Message);
            }
           
            return patientImageList;
        }

        public string GetImagePath(ImageInfoModel selectedImage)                             // DB에서 원본 영상 다운받고 원본이미지 주소 출력하는 함수
        {
            string path = string.Empty;

            try
            {
                if (null == _database)
                    throw new ApplicationException("database is null");

                var collection = _database.GetCollection<BsonDocument>("patientimageinfo");
                var filter = Builders<BsonDocument>.Filter.Eq("ImageUniqueName", selectedImage.ImageUniqueName);
                var cursor = collection.Find(filter).ToCursor();
                
                foreach (var document in cursor.ToEnumerable())
                {
                    DownloadImage(document["ImageID"].AsObjectId, document["ImageUniqueName"].ToString());

                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "PatientThumbnailImage", document["ImageUniqueName"].ToString() + ".png");
                }
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[GetImagePath] " + e.Message);
            }
            return path;
        }

        public bool DeleteAllImageInfoLIst(long patientNumber)                                             // 선택한 이미지정보 모두 삭제하는 함수
        {
            bool success = false;

            try
            {
                if (null == _database)
                    throw new ApplicationException("database is null");

                var collection = _database.GetCollection<BsonDocument>("patientimageinfo");
                var filter = Builders<BsonDocument>.Filter.Eq("PatientNumber", patientNumber);
                var cursor = collection.Find(filter).ToCursor();

                foreach (var imagedata in cursor.ToEnumerable())
                {
                    collection.DeleteOne(imagedata);
                }

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DeleteAllImageInfoLIst] " + e.Message);
            }

            return success;
        }
        public bool DeleteOneImageInfoLIst(string tImageUniqueName)                                        // 선택한 이미지 삭제하는 함수
        {
            bool success = false;

            try
            {
                if (null == _database)
                    throw new ApplicationException("database is null");

                var collection = _database.GetCollection<BsonDocument>("patientimageinfo");
                var filter = Builders<BsonDocument>.Filter.Eq("TimageUniqueName", tImageUniqueName);
                var cursor = collection.Find(filter).ToCursor();

                foreach (var imagedata in cursor.ToEnumerable())
                {
                    collection.DeleteOne(imagedata);
                }

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DeleteOneImageInfoLIst] " + e.Message);
            }

            return success;
        }
        //--------------------------------------------------------<환자 프로필 사진 DB>---------------------------------------
        public bool InsertPortrait(PatientInfoModel imageInfo)                                                   // 얼굴 사진 등록 하는 함수
        {
            bool success = false;

            try
            {
                if (null == imageInfo || null == _database)
                    throw new ApplicationException("imageInfo or database is null");

                var collection = _database.GetCollection<BsonDocument>("findpatientimageinfo");
                BsonDocument document = new BsonDocument()
                {
                    {"PatientNumber", imageInfo.PatientNumber},
                    {"PatientImagePath", imageInfo.PatientImagePath}
                };
                collection.InsertOne(document);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[InsertPortrait] " + e.Message);
            }

            return success;
        }

        public bool DeleteOnePortrait(long patientNumber)                                        // 환자 프로필 이미지 삭제하는 함수
        {
            bool success = false;

            try
            {
                if (null == _database)
                    throw new ApplicationException("database is null");

                var collection = _database.GetCollection<BsonDocument>("findpatientimageinfo");
                var filter = Builders<BsonDocument>.Filter.Eq("PatientNumber", patientNumber);
                var cursor = collection.Find(filter).ToCursor();

                foreach (var imagedata in cursor.ToEnumerable())
                {
                    collection.DeleteOne(imagedata);
                    break;
                }

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DeleteOnePortrait] " + e.Message);
            }

            return success;
        }

        public void UpdatePortrait(long patientNumber)
        {
            var collection = _database.GetCollection<BsonDocument>("findpatientimageinfo");
            var filter = Builders<BsonDocument>.Filter.Eq("PatientNumber", patientNumber);
            var count = collection.Count(filter);
            var cursor = collection.Find(filter).ToCursor();
            var document = cursor.FirstOrDefault();

            if (count > 1)
            {
                collection.DeleteOne(document);
            }
        }

        public string GetPortraitByPatientNumber(long patientNumber)                                                    // 얼굴 사진 불러오는 함수
        {
            string result = null;
            var collection = _database.GetCollection<BsonDocument>("findpatientimageinfo");
            var filter = Builders<BsonDocument>.Filter.Eq("PatientNumber", patientNumber);
            var cursor = collection.Find(filter).ToCursor();
            var document = cursor.FirstOrDefault();

            if (null != document)
            {
                result = document["PatientImagePath"].ToString();
            }  

            return result;
        }

        //-------------------------------------------------------<환자 정보 DB>-----------------------------------------------------
        public bool InsertPatientInfo(PatientInfoModel patientInfo)                                                     // 입력한 정보 등록하는 함수
        {
            bool success = false;

            try
            {
                if (null == patientInfo || null == _database)
                    throw new ApplicationException("patientInfo or database is null");

                var collection = _database.GetCollection<BsonDocument>("patientinfo");
                long count = GetNewPatientNumber();
                
                if (count < 0)
                    return success;

                BsonDocument document = new BsonDocument()
                {
                    {"PatientNumber", patientInfo.PatientNumber = count},
                    {"PatientName", patientInfo.PatientName},
                    {"PatientResnum", patientInfo.PatientResnum},
                    {"PatientBirthday", patientInfo.PatientBirthday},
                    {"PatientPhonenum", patientInfo.PatientPhonenum}
                };
                collection.InsertOne(document);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[InsertPatientInfo] " + e.Message);
            }

            return success;
        }

        public bool EditPatientInfo(long patientNumber, PatientInfoModel patientInfo)                   // 선택한 정보 수정하는 함수
        {
            bool success = false;

            try
            {
                if (null == patientInfo || null == _database)
                    throw new ApplicationException("patientInfo or database is null");

                DeleteOnePatientInfoList(patientNumber);
                var collection = _database.GetCollection<BsonDocument>("patientinfo");
                BsonDocument document = new BsonDocument()
                {
                    {"PatientNumber", patientInfo.PatientNumber},
                    {"PatientName", patientInfo.PatientName},
                    {"PatientResnum", patientInfo.PatientResnum},
                    {"PatientBirthday", patientInfo.PatientBirthday},
                    {"PatientPhonenum", patientInfo.PatientPhonenum}
                };

                collection.InsertOne(document);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[EditPatientInfo] " + e.Message);
            }

            return success;
        }

        public List<PatientInfoModel> GetPatientInfoListByPatientName(string keyWord)                         // 검색한 정보 받아서 관련된 list 출력하는 함수
        {
            if (true == String.IsNullOrEmpty(keyWord))
                return null;

            List<PatientInfoModel> patientList = new List<PatientInfoModel>();

            try
            {
                if (null == _database)
                    throw new ApplicationException("database is null");

                var collection = _database.GetCollection<BsonDocument>("patientinfo");
                var filter = Builders<BsonDocument>.Filter.Regex("PatientName", keyWord + ".*");
                var cursor = collection.Find(filter).ToCursor();

                foreach (var document in cursor.ToEnumerable())
                {
                    PatientInfoModel patient = new PatientInfoModel();
                    patient.PatientNumber = document["PatientNumber"].ToInt64();
                    patient.PatientName = document["PatientName"].ToString();
                    patient.PatientResnum = document["PatientResnum"].ToString();
                    patient.PatientBirthday = document["PatientBirthday"].ToString();
                    patient.PatientPhonenum = document["PatientPhonenum"].ToString();

                    patientList.Add(patient);
                }
            }
            catch (Exception e)
            { 
                SimpleLogger.Instance()._OutputErrorMsg("[GetPatientInfoListByPatientName] " + e.Message);
            }

            return patientList;
        }

        public bool DeleteOnePatientInfoList(long patientNumber)                                                 // 선택한 정보 삭제하는 함수
        {
            bool success = false;

            try
            {
                if (null == _database)
                    throw new ApplicationException("database is null");

                var collection = _database.GetCollection<BsonDocument>("patientinfo");
                var filter = Builders<BsonDocument>.Filter.Eq("PatientNumber", patientNumber);
                var document = collection.Find(filter).FirstOrDefault();

                collection.DeleteOne(document);

                success = true;
            }
            catch (Exception e)
            {
                SimpleLogger.Instance()._OutputErrorMsg("[DeleteOnePatientInfoList] " + e.Message);
            }

            return success;
        }
    }
}