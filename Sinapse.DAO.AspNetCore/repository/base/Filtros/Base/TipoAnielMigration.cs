namespace Sinapse.DAO.AspNetCore
{
    public enum TipoAnielMigration
    {
        DDL = 0,
        ProcedureTrigger = 1,
        ScriptUnico = 2,
        ScriptMultiplo = 3,
        Select = 4
    }
}
