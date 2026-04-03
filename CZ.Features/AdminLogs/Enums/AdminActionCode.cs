namespace api.CZ.Features.AdminLogs.Enums;

public enum AdminActionCode
{
    // Administrator actions
    ADMIN_CREATED,
    ADMIN_UPDATED,
    ADMIN_DELETED,

    // Information Page actions
    INFO_PAGE_CREATED,
    INFO_PAGE_UPDATED,
    INFO_PAGE_PUBLISHED,
    INFO_PAGE_DELETED,

    // Information Tag actions
    INFO_TAG_CREATED,
    INFO_TAG_UPDATED,
    INFO_TAG_DELETED,

    // Navigation Menu actions
    NAV_MENU_CREATED,
    NAV_MENU_UPDATED,
    NAV_MENU_DELETED,

    // Configuration actions
    CONFIG_CREATED,
    CONFIG_UPDATED,
    CONFIG_DELETED,

    // Quiz actions
    QUIZ_CREATED,
    QUIZ_UPDATED,
    QUIZ_DELETED,

    // User management actions
    USER_ENABLED,
    USER_DISABLED,
    USER_SESSION_REVOKED,

    // Other actions
    PASSWORD_CHANGED,
    SESSION_REVOKED,
    BULK_OPERATION
}
