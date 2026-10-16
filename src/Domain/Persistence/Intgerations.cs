using System;
using System.Security.Cryptography;
using System.Text;

public static class ShopeeAuthHelper
{
    /// <summary>
    /// Gera a assinatura (sign) exigida pela API v2 da Shopee usando HMAC-SHA256.
    /// </summary>
    /// <param name="partnerId">Seu ID de parceiro (ex: 123456 ou ID de teste)</param>
    /// <param name="path">O caminho do endpoint (ex: /api/v2/order/get_order_list)</param>
    /// <param name="timestamp">Timestamp Unix atual (em segundos)</param>
    /// <param name="partnerKey">Sua chave secreta de parceiro</param>
    /// <param name="accessToken">Token de acesso OAuth (Opcional, dependendo da API)</param>
    /// <param name="shopId">ID da loja (Opcional, para APIs de Loja)</param>
    /// <returns>Hash HMAC-SHA256 em formato hexadecimal minúsculo</returns>
    public static string GenerateSign(long partnerId, string path, long timestamp, string partnerKey, string accessToken = null, long? shopId = null)
    {
        var sb = new StringBuilder();
        
        // 1. Ordem estrita exigida pela Shopee para a Base String:
        // PartnerId + Path + Timestamp [+ AccessToken] [+ ShopId]
        sb.Append(partnerId);
        sb.Append(path);
        sb.Append(timestamp);

        if (!string.IsNullOrEmpty(accessToken))
        {
            sb.Append(accessToken);
        }

        if (shopId.HasValue)
        {
            sb.Append(shopId.Value);
        }

        string baseString = sb.ToString();

        // 2. Calcula o HMAC-SHA256
        byte[] keyBytes = Encoding.UTF8.GetBytes(partnerKey);
        byte[] messageBytes = Encoding.UTF8.GetBytes(baseString);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            byte[] hashBytes = hmac.ComputeHash(messageBytes);
            
            // 3. Converte os bytes para string hexadecimal minúscula
            var sbHex = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sbHex.Append(b.ToString("x2")); // "x2" garante minúsculas
            }
            
            return sbHex.ToString();
        }
    }
}