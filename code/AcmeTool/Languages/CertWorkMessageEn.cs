namespace AcmeTool.Languages;


/// <inheritdoc/>
public class CertWorkMessageEn : ICertWorkMessage
{
    public string Main_Dk_ArgNone => "No decryption file specified";
    public string Main_Dk_NoFile => "The specified file does not exist";
    public string Main_Dk_ErrorLoad => "Failed to load key file";
    public string Main_Dk_Success => "Loaded successfully, key content as follows:";
    public string Main_Dk_LoadFail => "File cannot be loaded as a valid key";
    public string Main_WorkDirectory => "Current working directory:";

    public string ApplyWork_CheckApply_ScopedTitle => "Application Configuration Check";
    public string ApplyWork_CheckApply_Start => "Start Verifying Configuration";
    public string ApplyWork_CheckApply_Err_NoHostList => "Domain list is not configured";
    public string ApplyWork_CheckApply_Err_WildcardNotSupportHttp01 => "Wildcard domain [{0}] cannot use [{1}] authentication";
    public string ApplyWork_CheckApply_Err_IpNotSupportDns01 => "IP address [{0}] cannot use [{1}] authentication";
    public string ApplyWork_CheckApply_Err_InvalidChallengeType => "Authentication method [{1}] configured for [{0}] is invalid";
    public string ApplyWork_CheckApply_Err_NoChallengeDir => "HTTP01 challenge file storage directory is not configured";
    public string ApplyWork_CheckApply_Err_ChallengeDirNotExist => "HTTP01 challenge file storage directory does not exist";
    public string ApplyWork_CheckApply_Err_NoCertPath => "Certificate storage path is not configured!";
    public string ApplyWork_CheckApply_Err_CertDirNotExist => "Certificate storage directory does not exist!";
    public string ApplyWork_CheckApply_Err_NoCertKeyPath => "Certificate private key storage path is not configured!";
    public string ApplyWork_CheckApply_Err_CertKeyDirNotExist => "Certificate private key storage directory does not exist!";
    public string ApplyWork_CheckApply_Passed => "Configuration Verification Passed";
    public string ApplyWork_CheckHttp01_Scoped => "HTTP01 Verification";
    public string ApplyWork_CheckHttp01_Err_Http01ValueMismatch => "The value read from [{0}] does not match the expected value: Read value:\n{1}";
    public string ApplyWork_CheckHttp01_Err_Http01ReadFail => "Failed to read verification test file for [{0}]:\n{1}";
    public string ApplyWork_LoadAccount_Scope => "ACME Account";
    public string ApplyWork_LoadAccount_LoadExistAccountKey => "Create or load account information using existing private key";
    public string ApplyWork_LoadAccount_CreateNewAccountKey => "Generate new account private key";
    public string ApplyWork_LoadAccount_CreateNewSuccess => "Account obtained successfully";
    public string ApplyWork_CheckCurrent_Scoped => "Update Check";
    public string ApplyWork_CheckCurrent_Start => "Check status of existing certificate";
    public string ApplyWork_CheckCurrent_NoNeedUpdate => "Current certificate is valid, no update required";
    public string ApplyWork_CheckCurrent_NeedUpdate => "Certificate needs to be updated";
    public string ApplyWork_CheckCurrent_CheckLast => "Start checking certificate from last application";
    public string ApplyWork_CheckCurrent_NoLastRecord => "No valid application record found, start normal application process";
    public string ApplyWork_CheckCurrent_LoadLastRecord => "Previous application record detected, loading";
    public string ApplyWork_CheckCurrent_LastCertValidReplace => "Certificate from last application is still valid, replace current certificate";
    public string ApplyWork_CheckCurrent_LastCertInvalidUpdate => "Certificate from last application is invalid, trigger update";
    public string ApplyWork_CheckCurrent_FindUnfinishedRecord => "Unfinished previous application record found, check order status";
    public string ApplyWork_CheckCurrent_LastOrderReadFail => "Failed to read last certificate application info, start new application";
    public string ApplyWork_CheckCurrent_LastOrderNotContinue => "Previous certificate application cannot be resumed, start new application";
    public string ApplyWork_CheckCurrent_LastOrderCanContinue => "Previous certificate application can be resumed, continue application";
    public string ApplyWork_CheckCurrent_LastRecordInvalid => "Previous certificate application record is invalid, start new application";
    public string ApplyWork_CreateOrder_Scoped => "Order";
    public string ApplyWork_CreateOrder_Start => "Generate new order";
    public string ApplyWork_CreateOrder_OrderUrl => "Order address:";
    public string ApplyWork_ApplyCert_Scoped => "ACME";
    public string ApplyWork_ApplyCert_Err_ApplyCycleTimeout => "Certificate application not completed within specified cycle, forced termination";
    public string ApplyWork_ApplyCert_RefreshOrder => "Refresh order status";
    public string ApplyWork_ApplyCert_WaitGenerating => "Certificate generating";
    public string ApplyWork_ApplyCert_Err_OrderStatusInvalid => "Invalid certificate status: {0}";
    public string ApplyWork_ApplyCert_WaitNextCycle => "Current business cycle finished, wait 10s for next cycle";
    public string ApplyWork_CertKey_Scope => "Certificate Private Key";
    public string ApplyWork_CertKey_LoadExist => "Attempt to load existing private key";
    public string ApplyWork_CertKey_CreateNew => "Private key not found, generate new certificate private key";
    public string ApplyWork_DomainAuth_Scoped => "Domain Authentication";
    public string ApplyWork_DomainAuth_Start => "Start authentication for all domains";
    public string ApplyWork_DomainAuth_NotFound => "Failed to get authentication info for [{0}]!";
    public string ApplyWork_DomainAuth_StatusError => "Authentication failed: {0}";
    public string ApplyWork_DomainAuth_Challenge_Start => "Start Authentication";
    public string ApplyWork_DomainAuth_Http01_Err_NoChallenge => "[{0}] does not contain [http-01] challenge, application terminated";
    public string ApplyWork_DomainAuth_Http01_CreateVerifyFile => "Generate verification file";
    public string ApplyWork_DomainAuth_DnsPersist01_Err_NoChallenge => "[{0}] does not contain [dns-persist-01] challenge, application terminated";
    public string ApplyWork_DomainAuth_Err_Unsupported => "Selected authentication method is not supported";
    public string ApplyWork_DomainAuth_Challenge_Acknowledge => "Acknowledge challenge";
    public string ApplyWork_SendError_OrderError => "Invalid order:";
    public string ApplyWork_SendError_DomainError_Scoped => "Domain Authentication Error";
    public string ApplyWork_SendError_DomainError_AuthFail => "Domain authentication failed";
    public string ApplyWork_SubmitCsr_Scope => "Certificate Request";
    public string ApplyWork_SubmitCsr_Send => "Submit";
    public string ApplyWork_DownloadCert_Scoped => "Certificate Download";
    public string ApplyWork_DownloadCert_Start => "Start downloading certificate";
    public string ApplyWork_DownloadCert_Success => "Download completed";
    public string ApplyWork_CopyCert_Scope => "[Certificate Update]";
    public string ApplyWork_CopyCert_Err_LoadRecordCertFail => "Failed to read new certificate information";
    public string ApplyWork_CopyCert_Success => "New certificate valid period: [{0}] to [{1}]";

    public string LogRecorder_ErrorCode => "Error Code：";
    public string LogRecorder_ErrorDetail => "Error Detail：";
    public string LogRecorder_ErrorTitle => "Error Title：";

    public string ApplyCert_NoneAcme => "No ACME endpoint configured";

    public string ApplyCert_AcmeResponseException => "ACME server response error";

    public string ApplyCert_UnhandelException => "Unhandled exception";

    public string ApplySettting_Path => "Configuration file path: ";

    public string ApplySettting_Created => "The configuration file does not exist; a default configuration template has been generated at the specified configuration path";

    public string Webhook_Test_ConfigInvalid => "Invalid Webhook configuration, please verify";

    public string Message_Test_WebhookHint => "Webhook test message sent successfully, please check receipt";

    public string Message_Test_Content => "This is a test message from AcmeTool";

    public string Message_CertUpdate_SuccessTitle => "AcmeTool Certificate Renewal Success Notification";

    public string Message_CertUpdate_FailTitle => "AcmeTool Certificate Renewal Failure Notification";

    public string Message_CertUpdate_SuccessMessage => "The certificate applicable to {0} has been renewed successfully.";

    public string Message_CertUpdate_FailMassage => "Failed to renew the certificate applicable to {0}.";

    public string Message_CertUpdate_DetailTitle => "Details: ";

    public string Message_WebhookSendFailed => "Failed to send Webhook message";

    public string EmailConfigInvalidTip => "Invalid Email configuration, please verify";

    public string Message_Test_EmailHint => "Email message sent successfully, please check receipt";

    public string Message_EmailSendFailed => "Failed to send email message";

    public string Console_ExitWait => "(Press Enter to terminate the application, or it will exit automatically after 10 minutes)";

    public string DnsPersist01_Ca_UnSupport => "CA does not support the dns-persist-01 challenge";

    public string DnsPersist01_InputName => "Please enter the domain name for this record: [*] means all domains";

    public string DnsPersist01_InputWildcard => "Authorize wildcard domain? [y] The domain of the TXT record and its subdomains; [n] Only the domain of the TXT record";

    public string DnsPersist01_InputWildcard_Special => "Authorize wildcard domain? [y] {0} and its subdomains; [n] Only {0}";

    public string DnsPersist01_InputValidateTime => "Enter authorization expiration time: leave blank for no limit, ISO8601/RFC3339 format";

    public string DnsPersist01_TxtResult => "TXT record value is as follows";
}