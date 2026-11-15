namespace AcmeTool.Languages;

public interface ICertWorkMessage
{
    /// <summary>
    /// 加载密钥文件出错
    /// </summary>
    string Main_Dk_ErrorLoad { get; }
    /// <summary>
    /// 未指定要解密的文件
    /// </summary>
    string Main_Dk_ArgNone { get; }
    /// <summary>
    /// 文件无法加载为有效密钥
    /// </summary>
    string Main_Dk_LoadFail { get; }
    /// <summary>
    /// 指定文件不存在
    /// </summary>
    string Main_Dk_NoFile { get; }
    /// <summary>
    /// 加载成功
    /// </summary>
    string Main_Dk_Success { get; }
    /// <summary>
    /// 当前工作目录
    /// </summary>
    string Main_WorkDirectory { get; }

    #region 配置检查模块
    /// <summary>
    /// 申请配置检查
    /// </summary>
    string ApplyWork_CheckApply_ScopedTitle { get; }
    /// <summary>
    /// 开始校验配置
    /// </summary>
    string ApplyWork_CheckApply_Start { get; }
    /// <summary>
    /// 未配置域名列表
    /// </summary>
    string ApplyWork_CheckApply_Err_NoHostList { get; }
    /// <summary>
    /// 泛域名【{0}】不可使用【{1}】认证
    /// </summary>
    string ApplyWork_CheckApply_Err_WildcardNotSupportHttp01 { get; }
    /// <summary>
    /// IP地址【{0}】不可使用【{1}】认证
    /// </summary>
    string ApplyWork_CheckApply_Err_IpNotSupportDns01 { get; }
    /// <summary>
    /// 【{0}】配置的认证方式【{1}】无效
    /// </summary>
    string ApplyWork_CheckApply_Err_InvalidChallengeType { get; }
    /// <summary>
    /// 未配置HTTP01挑战文件存放目录
    /// </summary>
    string ApplyWork_CheckApply_Err_NoChallengeDir { get; }
    /// <summary>
    /// HTTP01挑战文件存放目录不存在
    /// </summary>
    string ApplyWork_CheckApply_Err_ChallengeDirNotExist { get; }
    /// <summary>
    /// 未配置证书存放路径！
    /// </summary>
    string ApplyWork_CheckApply_Err_NoCertPath { get; }
    /// <summary>
    /// 证书存放目录不存在！
    /// </summary>
    string ApplyWork_CheckApply_Err_CertDirNotExist { get; }
    /// <summary>
    /// 未配置证书密钥存放路径！
    /// </summary>
    string ApplyWork_CheckApply_Err_NoCertKeyPath { get; }
    /// <summary>
    /// 证书密钥存放目录不存在！
    /// </summary>
    string ApplyWork_CheckApply_Err_CertKeyDirNotExist { get; }
    /// <summary>
    /// 配置校验通过
    /// </summary>
    string ApplyWork_CheckApply_Passed { get; }
    #endregion

    #region HTTP01校验模块
    /// <summary>
    /// HTTP01校验
    /// </summary>
    string ApplyWork_CheckHttp01_Scoped { get; }
    /// <summary>
    /// 【{0}】读取到的值与期望值不匹配：读取到的值为：\n{1}
    /// </summary>
    string ApplyWork_CheckHttp01_Err_Http01ValueMismatch { get; }
    /// <summary>
    /// 【{0}】读取验证测试文件失败：\n{1}
    /// </summary>
    string ApplyWork_CheckHttp01_Err_Http01ReadFail { get; }
    #endregion

    #region ACME账户加载模块
    /// <summary>
    /// ACME账户
    /// </summary>
    string ApplyWork_LoadAccount_Scope { get; }
    /// <summary>
    /// 以已存在的密钥创建或加载账户信息
    /// </summary>
    string ApplyWork_LoadAccount_LoadExistAccountKey { get; }
    /// <summary>
    /// 创建新的账户密钥
    /// </summary>
    string ApplyWork_LoadAccount_CreateNewAccountKey { get; }
    /// <summary>
    /// 已成功获取账户
    /// </summary>
    string ApplyWork_LoadAccount_CreateNewSuccess { get; }
    #endregion

    #region 证书状态检查模块
    /// <summary>
    /// 更新检查
    /// </summary>
    string ApplyWork_CheckCurrent_Scoped { get; }
    /// <summary>
    /// 检查现有证书状态
    /// </summary>
    string ApplyWork_CheckCurrent_Start { get; }
    /// <summary>
    /// 现有证书有效，无需更新
    /// </summary>
    string ApplyWork_CheckCurrent_NoNeedUpdate { get; }
    /// <summary>
    /// 需要更新证书
    /// </summary>
    string ApplyWork_CheckCurrent_NeedUpdate { get; }
    /// <summary>
    /// 开始检查上一次申请的证书
    /// </summary>
    string ApplyWork_CheckCurrent_CheckLast { get; }
    /// <summary>
    /// 未找到有效申请记录，开始正常申请流程
    /// </summary>
    string ApplyWork_CheckCurrent_NoLastRecord { get; }
    /// <summary>
    /// 发现上次申请记录，加载中
    /// </summary>
    string ApplyWork_CheckCurrent_LoadLastRecord { get; }
    /// <summary>
    /// 上次申请的证书还在有效期中，替换现有证书
    /// </summary>
    string ApplyWork_CheckCurrent_LastCertValidReplace { get; }
    /// <summary>
    /// 上次申请的证书不可使用，启动更新
    /// </summary>
    string ApplyWork_CheckCurrent_LastCertInvalidUpdate { get; }
    /// <summary>
    /// 发现上次未完成的申请记录，检查订单状态
    /// </summary>
    string ApplyWork_CheckCurrent_FindUnfinishedRecord { get; }
    /// <summary>
    /// 无法读取上次证书申请信息，开始新的申请
    /// </summary>
    string ApplyWork_CheckCurrent_LastOrderReadFail { get; }
    /// <summary>
    /// 上次证书申请不可继续，开始新的申请
    /// </summary>
    string ApplyWork_CheckCurrent_LastOrderNotContinue { get; }
    /// <summary>
    /// 上次证书申请可以继续，继续申请
    /// </summary>
    string ApplyWork_CheckCurrent_LastOrderCanContinue { get; }
    /// <summary>
    /// 上次证书申请记录无效，开始新的申请
    /// </summary>
    string ApplyWork_CheckCurrent_LastRecordInvalid { get; }
    #endregion

    #region 订单创建模块
    /// <summary>
    /// 订单
    /// </summary>
    string ApplyWork_CreateOrder_Scoped { get; }
    /// <summary>
    /// 生成新订单
    /// </summary>
    string ApplyWork_CreateOrder_Start { get; }
    /// <summary>
    /// 订单地址为：
    /// </summary>
    string ApplyWork_CreateOrder_OrderUrl { get; }
    #endregion

    #region 证书申请主流程模块
    /// <summary>
    /// ACME
    /// </summary>
    string ApplyWork_ApplyCert_Scoped { get; }
    /// <summary>
    /// 证书申请未在指定的周期内完成，强行终止
    /// </summary>
    string ApplyWork_ApplyCert_Err_ApplyCycleTimeout { get; }
    /// <summary>
    /// 刷新订单状态
    /// </summary>
    string ApplyWork_ApplyCert_RefreshOrder { get; }
    /// <summary>
    /// 证书生成中
    /// </summary>
    string ApplyWork_ApplyCert_WaitGenerating { get; }
    /// <summary>
    /// 证书状态错误：{0}
    /// </summary>
    string ApplyWork_ApplyCert_Err_OrderStatusInvalid { get; }
    /// <summary>
    /// 本轮业务处理完成，等待10s进行下一轮处理
    /// </summary>
    string ApplyWork_ApplyCert_WaitNextCycle { get; }
    #endregion

    #region 证书密钥生成模块
    /// <summary>
    /// 证书密钥
    /// </summary>
    string ApplyWork_CertKey_Scope { get; }
    /// <summary>
    /// 尝试读取已存在的密钥
    /// </summary>
    string ApplyWork_CertKey_LoadExist { get; }
    /// <summary>
    /// 密钥不存在，生成新的证书密钥
    /// </summary>
    string ApplyWork_CertKey_CreateNew { get; }
    #endregion

    #region 域名认证模块
    /// <summary>
    /// 域名认证
    /// </summary>
    string ApplyWork_DomainAuth_Scoped { get; }
    /// <summary>
    /// 开始对所有域名进行认证
    /// </summary>
    string ApplyWork_DomainAuth_Start { get; }
    /// <summary>
    /// 获取【{0}】认证信息失败！
    /// </summary>
    string ApplyWork_DomainAuth_NotFound { get; }
    /// <summary>
    /// 认证失败：{0}
    /// </summary>
    string ApplyWork_DomainAuth_StatusError { get; }
    /// <summary>
    /// 开始认证
    /// </summary>
    string ApplyWork_DomainAuth_Challenge_Start { get; }
    /// <summary>
    /// 【{0}】不包含【http-01】认证，申请终止
    /// </summary>
    string ApplyWork_DomainAuth_Http01_Err_NoChallenge { get; }
    /// <summary>
    /// 生成验证文件
    /// </summary>
    string ApplyWork_DomainAuth_Http01_CreateVerifyFile { get; }
    /// <summary>
    /// 【{0}】不包含【dns-persist-01】认证，申请终止
    /// </summary>
    string ApplyWork_DomainAuth_DnsPersist01_Err_NoChallenge { get; }
    /// <summary>
    /// 设置的申请方式不在支持的范围中
    /// </summary>
    string ApplyWork_DomainAuth_Err_Unsupported { get; }
    /// <summary>
    /// 提交挑战
    /// </summary>
    string ApplyWork_DomainAuth_Challenge_Acknowledge { get; }
    #endregion

    #region 订单错误处理模块
    /// <summary>
    /// 订单无效：
    /// </summary>
    string ApplyWork_SendError_OrderError { get; }
    /// <summary>
    /// 域名认证错误
    /// </summary>
    string ApplyWork_SendError_DomainError_Scoped { get; }
    /// <summary>
    /// 域名认证失败
    /// </summary>
    string ApplyWork_SendError_DomainError_AuthFail { get; }
    #endregion

    #region CSR提交模块
    /// <summary>
    /// 证书请求
    /// </summary>
    string ApplyWork_SubmitCsr_Scope { get; }
    /// <summary>
    /// 提交
    /// </summary>
    string ApplyWork_SubmitCsr_Send { get; }
    #endregion

    #region 证书下载模块
    /// <summary>
    /// 证书下载
    /// </summary>
    string ApplyWork_DownloadCert_Scoped { get; }
    /// <summary>
    /// 开始下载证书
    /// </summary>
    string ApplyWork_DownloadCert_Start { get; }
    /// <summary>
    /// 下载完成
    /// </summary>
    string ApplyWork_DownloadCert_Success { get; }
    #endregion

    #region 证书复制更新模块
    /// <summary>
    /// 【证书更新】
    /// </summary>
    string ApplyWork_CopyCert_Scope { get; }
    /// <summary>
    /// 无法读取新证书信息
    /// </summary>
    string ApplyWork_CopyCert_Err_LoadRecordCertFail { get; }
    /// <summary>
    /// 新证书有效期为【{0}】至【{1}】
    /// </summary>
    string ApplyWork_CopyCert_Success { get; }
    #endregion

    /// <summary>
    /// 错误代码
    /// </summary>
    string LogRecorder_ErrorCode { get; }
    /// <summary>
    /// 错误详情
    /// </summary>
    string LogRecorder_ErrorDetail { get; }
    /// <summary>
    /// 错误标题
    /// </summary>
    string LogRecorder_ErrorTitle { get; }
    /// <summary>
    /// 未配置 Acme 端点
    /// </summary>
    string ApplyCert_NoneAcme { get; }
    /// <summary>
    /// ACME服务器响应错误
    /// </summary>
    string ApplyCert_AcmeResponseException { get; }
    /// <summary>
    /// 未处理的异常
    /// </summary>
    string ApplyCert_UnhandelException { get; }
    /// <summary>
    /// 配置文件路径
    /// </summary>
    string ApplySettting_Path { get; }
    /// <summary>
    /// 配置文件不存在，已在配置文件路径生成默认配置模板
    /// </summary>
    string ApplySettting_Created { get; }

    /// <summary>
    /// Webhook 配置无效，请检查
    /// </summary>
    string Webhook_Test_ConfigInvalid { get; }

    /// <summary>
    /// Webhook 消息已发送，请查收
    /// </summary>
    string Message_Test_WebhookHint { get; }

    /// <summary>
    /// 这是一条来自AcmeTool的测试消息
    /// </summary>
    string Message_Test_Content { get; }

    /// <summary>
    /// AcmeTool 证书更新成功通知
    /// </summary>
    string Message_CertUpdate_SuccessTitle { get; }

    /// <summary>
    /// AcmeTool 证书更新失败通知
    /// </summary>
    string Message_CertUpdate_FailTitle { get; }

    /// <summary>
    /// 可使用者为{0}的证书已更新成功。
    /// </summary>
    string Message_CertUpdate_SuccessMessage { get; }

    /// <summary>
    /// 可使用者为{0}的证书更新失败。
    /// </summary>
    string Message_CertUpdate_FailMassage { get; }

    /// <summary>
    /// 详细信息
    /// </summary>
    string Message_CertUpdate_DetailTitle { get; }

    /// <summary>
    /// 发送Webhook失败
    /// </summary>
    string Message_WebhookSendFailed { get; }

    /// <summary>
    /// Email 配置无效，请检查
    /// </summary>
    string EmailConfigInvalidTip { get; }

    /// <summary>
    /// Email 消息已发送，请查收
    /// </summary>
    string Message_Test_EmailHint { get; }

    /// <summary>
    /// 发送Email失败
    /// </summary>
    string Message_EmailSendFailed { get; }

    /// <summary>
    /// （输入回车终止应用程序，或10分钟自动退出）
    /// </summary>
    string Console_ExitWait { get; }
    /// <summary>
    /// CA 不支持 dns-persist-01 挑战
    /// </summary>
    string DnsPersist01_Ca_UnSupport { get; }
    /// <summary>
    /// 请输入可使用该记录对应的域名：【*】表示全部域名
    /// </summary>
    string DnsPersist01_InputName { get; }
    /// <summary>
    /// 是否授权泛域名：【y】Txt记录所在域名及子域名；【n】仅Txt记录所在域名
    /// </summary>
    string DnsPersist01_InputWildcard { get; }
    /// <summary>
    /// 是否授权泛域名：【y】{0}及下级域名；【n】仅{0}
    /// </summary>
    string DnsPersist01_InputWildcard_Special { get; }
    /// <summary>
    /// 输入授权有效时间：空表示不限制，ISO8601/RFC3339格式
    /// </summary>
    string DnsPersist01_InputValidateTime { get; }
    /// <summary>
    /// Txt记录值如下
    /// </summary>
    string DnsPersist01_TxtResult { get; }
}