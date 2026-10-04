namespace AB.SmokPrzewodnik.Common;

public static class AppConsts
{
    public static string AuthPolicyName = "SupabaseUser";
    public static string NavigationAuthPolicyName = "NavigationTokenUser";

    public static TimeSpan NavigationTokenValidityDuration = TimeSpan.FromHours(1);
}
