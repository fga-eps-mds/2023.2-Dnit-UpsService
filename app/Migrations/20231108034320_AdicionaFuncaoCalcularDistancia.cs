using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace app.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaFuncaoCalcularDistancia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE OR REPLACE 
            FUNCTION public.""CalcularDistancia""(
                lat1 DOUBLE PRECISION,
                long1 DOUBLE PRECISION,
                lat2 DOUBLE PRECISION,
                long2 DOUBLE PRECISION
            )
            RETURNS DOUBLE PRECISION AS $$
            DECLARE 
                raioTerraKm CONSTANT DOUBLE PRECISION := 6371.0;
                distancia DOUBLE PRECISION := 0.0;
                difLat DOUBLE PRECISION;
                difLong DOUBLE PRECISION;
                radicando DOUBLE PRECISION;
                raiz DOUBLE PRECISION;
            BEGIN
                difLat = radians(lat2 - lat1);
                difLong = radians(long2 - long1);
                
                radicando = sin(difLat/2) * sin(difLat/2)
                    + cos(radians(lat1)) * cos(radians(lat2))
                    * sin(difLong/2) * sin(difLong/2);
                raiz = 2 * atan2(sqrt(radicando), sqrt(1-radicando));
                
                distancia = raioTerraKm * raiz; 
                RETURN distancia;
            END; $$ language plpgsql;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION public.""CalcularDistancia""");
        }
    }
}
