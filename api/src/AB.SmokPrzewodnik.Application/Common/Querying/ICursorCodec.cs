namespace AB.SmokPrzewodnik.Application.Common.Querying;

public interface ICursorCodec
{
    string Encode<TCursor>(TCursor cursor)
    where TCursor : class, ICursorPayload;

    bool TryDecode<TCursor>(string encoded,
        string expectedScope,
        string expectedFilterHash,
        out TCursor? cursor)
    where TCursor : class, ICursorPayload;
}
