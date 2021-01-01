using System;
using Miyabi.Asset.Models;
using Miyabi.Binary.Models;
using Miyabi.NFT.Models;
using Miyabi.Common.Models;
using Miyabi.ContractSdk;
using Miyabi.Contract.Models;
using Miyabi.ModelSdk.Execution;

namespace Miyabi.Tests.SCs
{
    public class Inheritance : ContractBase
    {
        static string HeritageAssetTable = "HeritageAsset";                 //金融資産の管理テーブル(AssetTable)
        static string TangibleAssetNFTTable = "TangibleAssetNFT";           //有形資産（土地の権利書、不動産、有価証券、その他遺産の権利系）(NFTTable)
        static string TestamentNFTTable = "TestamentNFT";                   //遺言状の管理テーブル (NFTTable)
        static string InheritanceManagementTable = "InheritanceManagement"; //相続情報及び遺言者の生存管理テーブル(binaryTable)

        public Inheritance(ContractInitializationContext ctx) : base(ctx)
        {

        }

        /// <summary>
        /// SmartContract Instance
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override bool Instantiate(string[] args)
        {
            //Assettableownerkwy = contract admin key 
            var contractAdmin = new[]
            {
                GetContractAddress(),
            };

            var heritageAssetTableName = GetHeritageAssetTableName();
            //AssetDiscripter(tablename,tracked ,proof,contractadmin(tableowner))
            var assettableDescriptor = new AssetTableDescriptor(heritageAssetTableName, false,false,contractAdmin);

            var tangibleAssetNFTTableName = GetTangibleAssetNFTTableName();
            //NFTDiscripter(tablename,tracked ,proof,contractadmin(tableowner))
            var NFTtangibleassetDescripter = new NFTTableDescriptor(tangibleAssetNFTTableName, false, false, contractAdmin);

            var testamentNFTTableName = GetTestamentNFTTableName();
            //NFTDiscripter(tablename,tracked ,proof,contractadmin(tableowner))
            var NFTTestamentDiscripter = new NFTTableDescriptor(testamentNFTTableName, false, false, contractAdmin);

            var inheritanceManagementTableName = GetInheritanceManagementTableName();
            //BinarytableDescriptor(tablename,tracks)
            var binarytableDescriptor = new BinaryTableDescriptor(inheritanceManagementTableName, false);

            try
            {
                //statewrite is environment hold
                StateWriter.AddTable(assettableDescriptor);
                StateWriter.AddTable(NFTtangibleassetDescripter);
                StateWriter.AddTable(NFTTestamentDiscripter);
                StateWriter.AddTable(binarytableDescriptor);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 相続可能性のある親族に関する情報登録
        /// </summary>
        /// <param name="RelativesAddress">親族の公開鍵</param>
        public void registrelatives(Address RelativesAddress,string Familyname)
        {
            var inheritanceManagementTableName = GetInheritanceManagementTableName();
            //TryGetTableWriter:(StateWriterに登録されたテーブルがあればtrue)
            if (!StateWriter.TryGetTableWriter<IBinaryTableWriter>(inheritanceManagementTableName, out var managementTable))   //happen false
            {
                return;
            }

            //すでに登録されているかチェック
            if (managementTable.TryGetValue(RelativesAddress.Encoded, out var value))
            {
                return;
            }

            //Binary values ​​are set in the participant table
            managementTable.SetValue(RelativesAddress.Encoded, ByteString.Parse(Familyname));
        }


        /// <summary>
        /// Deposit HeritageAsset method (現金、預金の財産をスマートコントラクトに預ける)
        /// </summary>
        /// <param name="TestatorAddress"></param>
        /// <param name="deposit"></param>
        /// <param name="Familyname"></param>
        public void depositheritageasset(Address TestatorAddress,decimal deposit,string Familyname)
        {
            var InheritanceManagement = GetInheritanceManagementTableName();
            if(!StateWriter.TryGetTableWriter<IBinaryTableWriter>(InheritanceManagement, out var InheritancemanageTable))
            {
                return;
            }
            var keyinfo = Familyname + "_HeritageAsset";
            var amount = Convert.ToString(deposit);
            //スマートコントラクトに預けた〇〇家の資産枚数を登録
            InheritancemanageTable.SetValue(ByteString.Encode(keyinfo), ByteString.Encode(amount));

            var HeritageAssetTable = GetHeritageAssetTableName();
            if (!StateWriter.TryGetTableWriter<IAssetTableWriter>(HeritageAssetTable, out var Table))   //happen false
            {
                return;
            }

            Table.MoveValue(TestatorAddress, GetContractAddress(),deposit);
        }

        /// <summary>
        /// Deposit Tangible Asset NFT method(有形資産の不動産、土地の権利書、有価証券などをスマートコントラクトにあづける)
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="Familyname"></param>
        public void deposittangibleassetNFT(string tokenId,string Familyname)
        {
            var InheritanceManagement = GetInheritanceManagementTableName();
            if (!StateWriter.TryGetTableWriter<IBinaryTableWriter>(InheritanceManagement, out var InheritancemanageTable))
            {
                return;
            }
            var keyinfo = Familyname + "_TangibleAsset";
            //スマートコントラクトに預けた〇〇家の有形資産の登録
            InheritancemanageTable.SetValue(ByteString.Encode(keyinfo), ByteString.Encode(tokenId));

            var TangibleAssetNFTTable = GetTangibleAssetNFTTableName();
            if (!StateWriter.TryGetTableWriter<INFTTableWriter>(TangibleAssetNFTTable, out var Table))   
            {
                return;
            }

            Table.TransferToken(tokenId, GetContractAddress());
        }

        /// <summary>
        /// Deposit Testament NFT method(遺言状をスマートコントラクトにあづける)
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="Familyname"></param>
        public void deposittestamentNFT(string tokenId,string Familyname)
        {
            var InheritanceManagement = GetInheritanceManagementTableName();
            if (!StateWriter.TryGetTableWriter<IBinaryTableWriter>(InheritanceManagement, out var InheritancemanageTable))
            {
                return;
            }
            var keyinfo = Familyname + "_Testament";
            //スマートコントラクトに預けた〇〇家の遺言状情報の登録
            InheritancemanageTable.SetValue(ByteString.Encode(keyinfo), ByteString.Encode(tokenId));

            var TestamentNFTTable = GetTestamentNFTTableName();
            if (!StateWriter.TryGetTableWriter<INFTTableWriter>(TestamentNFTTable, out var Table))
            {
                return;
            }

            Table.TransferToken(tokenId, GetContractAddress());
        }

        /// <summary>
        /// Disbursement Testament method(遺言状と相続権の払い出し)
        /// </summary>
        /// <param name="RequesterAddress"></param>
        /// <param name="Familyname"></param>
        public void Disbursementtestament(Address RequesterAddress, string Familyname)
        {
            var InheritanceManagement = GetInheritanceManagementTableName();
            if (!StateWriter.TryGetTableWriter<IBinaryTableWriter>(InheritanceManagement, out var InheritancemanageTable))
            {
                return;
            }

            //親族チェック
            if (!InheritancemanageTable.TryGetValue(RequesterAddress.Encoded, out var familyname))
            {
                return;
            }

            if(Familyname != (familyname.ToString()))
            {
                return;
            }

            //遺言状トークン情報（トークンID)の取得
            var keyinfo = Familyname + "_Testament";
            if (!InheritancemanageTable.TryGetValue(ByteString.Encode(keyinfo), out var tokenId))
            {
                return;
            }
            var tokeninfo = tokenId.ToString();

            //遺言者の死亡確認
            var Diedinfokey = Familyname + "_DiedTestatorAddress";
            if (!TryGetInternalValue(ByteString.Encode(Diedinfokey), out var TestatorAddress))
            {
                return;
            }

            //相続要求者の鍵一致確認
            var Inheritanceinfokey = Familyname + "_InheritanceAddress";
            if (!TryGetInternalValue(ByteString.Encode(Inheritanceinfokey), out var inheritanceAddress))
            {
                return;
            }
            var InheritanceAddress = PublicKeyAddress.Decode(inheritanceAddress);

            if(InheritanceAddress != RequesterAddress)
            {
                return;
            }

            //NFTテーブルの呼び出し
            var TestamentNFTTableName = GetTestamentNFTTableName();
            if(!StateWriter.TryGetTableWriter<INFTTableWriter>(TestamentNFTTableName,out var nfttable))
            {
                return;
            }
            nfttable.TransferToken(tokeninfo,RequesterAddress);
        }

        /// <summary>
        /// Inheritance Execution(遺産相続実行)
        /// </summary>
        /// <param name="RequesterAddress"></param>
        /// <param name="Familyname"></param>
        public void InheritanceExecution(Address RequesterAddress, string Familyname)
        {
            var InheritanceManagement = GetInheritanceManagementTableName();
            if (!StateWriter.TryGetTableWriter<IBinaryTableWriter>(InheritanceManagement, out var inheritancemanageTable))
            {
                return;
            }

            //相続要求者の鍵一致確認(NFTが盗まれたアカウント（鍵）からの要求の対策)
            var Inheritanceinfokey = Familyname + "_InheritanceAddress";
            if (!TryGetInternalValue(ByteString.Encode(Inheritanceinfokey), out var inheritanceAddress))
            {
                return;
            }
            var InheritanceAddress = PublicKeyAddress.Decode(inheritanceAddress);

            if (InheritanceAddress != RequesterAddress)
            {
                return;
            }

            var contractAddress = GetContractAddress();
            //金融資産相続情報の取得から相続まで
            var Assetinfokey = Familyname + "_HeritageAsset";
            if (inheritancemanageTable.TryGetValue(ByteString.Encode(Assetinfokey),out var deposit))
            {
                var amount = Convert.ToDecimal(deposit.ToString());
                var HeritageAssetTableName = GetHeritageAssetTableName();
                if (!StateWriter.TryGetTableWriter<IAssetTableWriter>(HeritageAssetTableName,out var heritageassettable))
                {
                    return;
                }
                heritageassettable.MoveValue(contractAddress, RequesterAddress, amount);
            }

            //有形資産相続情報の取得から相続まで
            var Tangibleinfokey = Familyname + "_TangibleAsset"; 
            if(inheritancemanageTable.TryGetValue(ByteString.Encode(Tangibleinfokey),out var tokenId))
            {
                var tokeninfo = tokenId.ToString();
                var TangibleAssetNFTTableName = GetTangibleAssetNFTTableName();
                if(!StateWriter.TryGetTableWriter<INFTTableWriter>(TangibleAssetNFTTableName,out var tangiblenftTable))
                {
                    return;
                }
                tangiblenftTable.TransferToken(tokeninfo, RequesterAddress);
            }
        }

        /// <summary>
        /// registInheritance method(遺言状の記載の相続者のアドレスを登録):将来的には本人 or 弁護士資格NFTを持つもののみが呼び出せる
        /// </summary>
        /// <param name="InheritanceAddress"></param>
        public void registInheritance(Address InheritanceAddress,string Familyname)
        {
            string keyinfo = Familyname + "_InheritanceAddress";
            SetInternalValue( ByteString.Encode(keyinfo), InheritanceAddress.Encoded);
        }

        /// <summary>
        /// Deathconfirmation method(このメソッドは弁護士のみが記帳可能(遺言者の死亡情報):将来的には弁護士資格NFTを持つもののみが呼び出せる)
        /// </summary>
        /// <param name="TestatorAddress"></param>
        public void Deathconfirmation(Address TestatorAddress,string Familyname)
        {
            string keyinfo = Familyname + "_DiedTestatorAddress";
            SetInternalValue(ByteString.Encode(keyinfo), TestatorAddress.Encoded);
        }

        /// <summary>
        /// Delete Infomation method(指定のバイナリテーブルの情報の値をnullを入れる)
        /// </summary>
        /// <param name="infomation"></param>
        public void DeleteInfomation(string infomation,string Familyname)
        {
            string keyinfo = Familyname + "_" +infomation;
            SetInternalValue(ByteString.Encode(keyinfo), null);
        }


        /// <summary>
        /// GenerateAssetTest method
        /// </summary>
        /// <param name="TestatorAddress"></param>
        /// <param name="amount"></param>
        public void GenerateAssetTest(Address TestatorAddress, decimal amount)
        {
            var HeritageAssetTableName = GetHeritageAssetTableName();
            if (!StateWriter.TryGetTableWriter<IAssetTableWriter>(HeritageAssetTableName, out var table))
            {
                return;
            }

            table.MoveValue(table.VoidAddress, TestatorAddress, amount);
        }

        /// <summary>
        /// GenerateTangibleAssetNFTTest method
        /// </summary>
        /// <param name="TestatorAddress"></param>
        /// <param name="tokenId"></param>
        public void GenerateTangibleAssetNFTTest(Address TestatorAddress, string tokenId)
        {
            var TangibleAssetNFTTableName = GetTangibleAssetNFTTableName();
            if (!StateWriter.TryGetTableWriter<INFTTableWriter>(TangibleAssetNFTTableName, out var table))
            {
                return;
            }

            table.GenerateToken(tokenId, TestatorAddress);
        }

        /// <summary>
        /// GenerateTestamentNFTTest method
        /// </summary>
        /// <param name="TestatorAddress"></param>
        /// <param name="tokenId"></param>
        public void GenerateTestamentNFTTest(Address TestatorAddress, string tokenId)
        {
            var TestamentNFTTableName = GetTestamentNFTTableName();
            if (!StateWriter.TryGetTableWriter<INFTTableWriter>(TestamentNFTTableName, out var table))
            {
                return;
            }

            table.GenerateToken(tokenId, TestatorAddress);
        }

        public bool TryGetInternalValue(ByteString key, out ByteString value)
        {
            return InternalTable.TryGetValue(key, out value);
        }

        public void SetInternalValue(ByteString key,ByteString value)
        {
            if(InternalTable is IPermissionedBinaryTableWriter internalTableWriter)
            {
                internalTableWriter.CreateOrUpdateValue(key, value);
            }
            else
            {
                throw new InvalidOperationException("Context is readOnly");
            }
        }

        //金融資産の管理テーブル(AssetTable)
        private string GetHeritageAssetTableName()
        {
            return AddInstanceSuffix(HeritageAssetTable);
        }

        //有形資産（土地の権利書、不動産、有価証券、その他遺産の権利系）(NFTTable)
        private string GetTangibleAssetNFTTableName()
        {
            return AddInstanceSuffix(TangibleAssetNFTTable);
        }

        //遺言状の管理テーブル (NFTTable)
        private string GetTestamentNFTTableName()
        {
            return AddInstanceSuffix(TestamentNFTTable);
        }

        //相続情報及び遺言者の生存管理テーブル(binaryTable)
        private string GetInheritanceManagementTableName()
        {
            return AddInstanceSuffix(InheritanceManagementTable);
        }

        private string AddInstanceSuffix(string tableName)
        {
            return tableName + InstanceName;
        }

        private Address GetContractAddress()
        {
            return ContractAddress.FromInstanceId(InstanceId);
        }
    }
}