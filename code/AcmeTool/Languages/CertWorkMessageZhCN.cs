namespace AcmeTool.Languages;

/// <inheritdoc/>
public class CertWorkMessageZhCN : ICertWorkMessage
{
    public string Main_Dk_ArgNone => "未指定要解密的文件";

    public string Main_Dk_NoFile => "指定文件不存在";
    public string Main_Dk_ErrorLoad => "加载密钥文件出错";
    public string Main_Dk_Success => "加载成功，密钥如下：";
    public string Main_Dk_LoadFail => "文件无法加载为有效密钥";
    public string Main_WorkDirectory => "当前工作目录：";
    public string ApplyWork_CheckApply_ScopedTitle => "申请配置检查";
    public string ApplyWork_CheckApply_Start => "开始校验配置";
    public string ApplyWork_CheckApply_Err_NoHostList => "未配置域名列表";
    public string ApplyWork_CheckApply_Err_WildcardNotSupportHttp01 => "泛域名【{0}】不可使用【{1}】认证";
    public string ApplyWork_CheckApply_Err_IpNotSupportDns01 => "IP地址【{0}】不可使用【{1}】认证";
    public string ApplyWork_CheckApply_Err_InvalidChallengeType => "【{0}】配置的认证方式【{1}】无效";
    public string ApplyWork_CheckApply_Err_NoChallengeDir => "未配置HTTP01挑战文件存放目录";
    public string ApplyWork_CheckApply_Err_ChallengeDirNotExist => "HTTP01挑战文件存放目录不存在";
    public string ApplyWork_CheckApply_Err_NoCertPath => "未配置证书存放路径！";
    public string ApplyWork_CheckApply_Err_CertDirNotExist => "证书存放目录不存在！";
    public string ApplyWork_CheckApply_Err_NoCertKeyPath => "未配置证书密钥存放路径！";
    public string ApplyWork_CheckApply_Err_CertKeyDirNotExist => "证书密钥存放目录不存在！";
    public string ApplyWork_CheckApply_Passed => "配置校验通过";
    public string ApplyWork_CheckHttp01_Scoped => "HTTP01校验";
    public string ApplyWork_CheckHttp01_Err_Http01ValueMismatch => "【{0}】读取到的值与期望值不匹配：读取到的值为：\n{1}";
    public string ApplyWork_CheckHttp01_Err_Http01ReadFail => "【{0}】读取验证测试文件失败：\n{1}";
    public string ApplyWork_LoadAccount_Scope => "ACME账户";
    public string ApplyWork_LoadAccount_LoadExistAccountKey => "以已存在的密钥创建或加载账户信息";
    public string ApplyWork_LoadAccount_CreateNewAccountKey => "创建新的账户密钥";
    public string ApplyWork_LoadAccount_CreateNewSuccess => "已成功获取账户";
    public string ApplyWork_CheckCurrent_Scoped => "更新检查";
    public string ApplyWork_CheckCurrent_Start => "检查现有证书状态";
    public string ApplyWork_CheckCurrent_NoNeedUpdate => "现有证书有效，无需更新";
    public string ApplyWork_CheckCurrent_NeedUpdate => "需要更新证书";
    public string ApplyWork_CheckCurrent_CheckLast => "开始检查上一次申请的证书";
    public string ApplyWork_CheckCurrent_NoLastRecord => "未找到有效申请记录，开始正常申请流程";
    public string ApplyWork_CheckCurrent_LoadLastRecord => "发现上次申请记录，加载中";
    public string ApplyWork_CheckCurrent_LastCertValidReplace => "上次申请的证书还在有效期中，替换现有证书";
    public string ApplyWork_CheckCurrent_LastCertInvalidUpdate => "上次申请的证书不可使用，启动更新";
    public string ApplyWork_CheckCurrent_FindUnfinishedRecord => "发现上次未完成的申请记录，检查订单状态";
    public string ApplyWork_CheckCurrent_LastOrderReadFail => "无法读取上次证书申请信息，开始新的申请";
    public string ApplyWork_CheckCurrent_LastOrderNotContinue => "上次证书申请不可继续，开始新的申请";
    public string ApplyWork_CheckCurrent_LastOrderCanContinue => "上次证书申请可以继续，继续申请";
    public string ApplyWork_CheckCurrent_LastRecordInvalid => "上次证书申请记录无效，开始新的申请";
    public string ApplyWork_CreateOrder_Scoped => "订单";
    public string ApplyWork_CreateOrder_Start => "生成新订单";
    public string ApplyWork_CreateOrder_OrderUrl => "订单地址为：";
    public string ApplyWork_ApplyCert_Scoped => "ACME";
    public string ApplyWork_ApplyCert_Err_ApplyCycleTimeout => "证书申请未在指定的周期内完成，强行终止";
    public string ApplyWork_ApplyCert_RefreshOrder => "刷新订单状态";
    public string ApplyWork_ApplyCert_WaitGenerating => "证书生成中";
    public string ApplyWork_ApplyCert_Err_OrderStatusInvalid => "证书状态错误：{0}";
    public string ApplyWork_ApplyCert_WaitNextCycle => "本轮业务处理完成，等待10s进行下一轮处理";
    public string ApplyWork_CertKey_Scope => "证书密钥";
    public string ApplyWork_CertKey_LoadExist => "尝试读取已存在的密钥";
    public string ApplyWork_CertKey_CreateNew => "密钥不存在，生成新的证书密钥";
    public string ApplyWork_DomainAuth_Scoped => "域名认证";
    public string ApplyWork_DomainAuth_Start => "开始对所有域名进行认证";
    public string ApplyWork_DomainAuth_NotFound => "获取【{0}】认证信息失败！";
    public string ApplyWork_DomainAuth_StatusError => "认证失败：{0}";
    public string ApplyWork_DomainAuth_Challenge_Start => "开始认证";
    public string ApplyWork_DomainAuth_Http01_Err_NoChallenge => "【{0}】不包含【http-01】认证，申请终止";
    public string ApplyWork_DomainAuth_Http01_CreateVerifyFile => "生成验证文件";
    public string ApplyWork_DomainAuth_DnsPersist01_Err_NoChallenge => "【{0}】不包含【dns-persist-01】认证，申请终止";
    public string ApplyWork_DomainAuth_Err_Unsupported => "设置的申请方式不在支持的范围中";
    public string ApplyWork_DomainAuth_Challenge_Acknowledge => "提交挑战";
    public string ApplyWork_SendError_OrderError => "订单无效：";
    public string ApplyWork_SendError_DomainError_Scoped => "域名认证错误";
    public string ApplyWork_SendError_DomainError_AuthFail => "域名认证失败";
    public string ApplyWork_SubmitCsr_Scope => "证书请求";
    public string ApplyWork_SubmitCsr_Send => "提交";
    public string ApplyWork_DownloadCert_Scoped => "证书下载";
    public string ApplyWork_DownloadCert_Start => "开始下载证书";
    public string ApplyWork_DownloadCert_Success => "下载完成";
    public string ApplyWork_CopyCert_Scope => "【证书更新】";
    public string ApplyWork_CopyCert_Err_LoadRecordCertFail => "无法读取新证书信息";
    public string ApplyWork_CopyCert_Success => "新证书有效期为【{0}】至【{1}】";

    public string LogRecorder_ErrorCode => "错误代码：";
    public string LogRecorder_ErrorDetail => "错误详情：";
    public string LogRecorder_ErrorTitle => "错误标题：";

    public string ApplyCert_NoneAcme => "未配置 Acme 端点";

    public string ApplyCert_AcmeResponseException => "ACME服务器响应错误";

    public string ApplyCert_UnhandelException => "未处理的异常";

    public string ApplySettting_Path => "配置文件路径：";

    public string ApplySettting_Created => "配置文件不存在，已在配置文件路径生成默认配置模板";

    public string Webhook_Test_ConfigInvalid => "Webhook 配置无效，请检查";

    public string Message_Test_WebhookHint => "Webhook 消息已发送，请查收";

    public string Message_Test_Content => "这是一条来自AcmeTool的测试消息";

    public string Message_CertUpdate_SuccessTitle => "AcmeTool 证书更新成功通知";

    public string Message_CertUpdate_FailTitle => "AcmeTool 证书更新失败通知";

    public string Message_CertUpdate_SuccessMessage => "可使用者为{0}的证书已更新成功。";

    public string Message_CertUpdate_FailMassage => "可使用者为{0}的证书更新失败。";

    public string Message_CertUpdate_DetailTitle => "详细信息：";

    public string Message_WebhookSendFailed => "发送Webhook失败";

    public string EmailConfigInvalidTip => "Email 配置无效，请检查";


    public string Message_Test_EmailHint => "Email 消息已发送，请查收";
    public string Message_EmailSendFailed => "发送Email失败";

    public string Console_ExitWait => "（输入回车终止应用程序，或10分钟自动退出）";

    public string DnsPersist01_Ca_UnSupport => "CA 不支持 dns-persist-01 挑战";

    public string DnsPersist01_InputName => "请输入可使用该记录对应的域名：【*】表示全部域名";

    public string DnsPersist01_InputWildcard => "是否授权泛域名：【y】Txt记录所在域名及子域名；【n】仅Txt记录所在域名";

    public string DnsPersist01_InputWildcard_Special => "是否授权泛域名：【y】{0}及下级域名；【n】仅{0}";

    public string DnsPersist01_InputValidateTime => "输入授权有效时间：空表示不限制，ISO8601/RFC3339格式";

    public string DnsPersist01_TxtResult => "Txt记录值如下";
}
