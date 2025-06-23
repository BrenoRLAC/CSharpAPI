namespace API.Constants
{
    public class EmailTemplate
    {
        public const string ForgotPassword = """
            <p><h4>Olá {0}!</h4></p>
            <p>Esta é uma senha temporária, apenas para que você possa alterar sua senha definitiva com mais segurança.</p>
            <p>
                Login: <strong>{1}</strong><br>
                Senha provisória: <strong>{2}</strong><br>
                Período de validade: <strong>3 minutos</strong>
            </p>
            <p>Atenciosamente,<br>Equipe Hero Corp.</p>
            <br>
            <p><small>Este é um e-mail automático, não há necessidade de respondê-lo.</small></p>
        """;

        public const string SecondAuth = """
            <p><h4>Olá {0}!</h4></p>
            <p>Este é o seu código temporário para voce conseguir efetuar seu login com segurança.</p>
            <p>
                 Login: <strong>{1}</strong><br>
                Token de autenticação: <strong>{2}</strong><br>
                Validade: <strong>3 minutos</strong>
            </p>
            <p>Atenciosamente,<br>Equipe Hero Corp.</p>
            <br>
            <p><small>Este é um e-mail automático, não há necessidade de respondê-lo.</small></p>
        """;

    }
}

