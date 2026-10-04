namespace AB.SmokPrzewodnik.Common;

public static class AppConsts
{
    public const string AuthPolicyName = "SupabaseUser";
    public const string NavigationAuthPolicyName = "NavigationTokenUser";
    public const string NavigationConnectionPolicyName = "NavigationConnection";

    public static TimeSpan NavigationTokenValidityDuration = TimeSpan.FromHours(1);
}
