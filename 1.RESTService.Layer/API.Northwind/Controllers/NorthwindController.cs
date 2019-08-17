using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.AnalysisServices.AdomdClient;

namespace API.Northwind.Controllers
{
    [EnableCors(origins:"*", headers:"*", methods:"*")]
    [RoutePrefix("v1/Dashboard/Northwind")]

    public class NorthwindController : ApiController
    {
        [HttpGet]
        [Route("Testing")]
        public HttpResponseMessage Testing()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Prueba de API Northwind exitosa!!!");
        }

        [HttpGet]
        [Route("Top5/{dimension}")]
        public HttpResponseMessage Top5(string dimension)
        {
            string nombreDimension = @"";

            switch (dimension)
            {
                case "1": // Clientes
                    nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].CHILDREN } ";
                    break;
                case "2": // Productos
                    nombreDimension = @"{ [Dim Producto].[Dim Producto Nombre].CHILDREN } ";
                    break;
                case "3": // Categorías
                    nombreDimension = @"{ [Dim Producto].[Dim Producto Categoria].CHILDREN } ";
                    break;
                case "4": // Empleados
                    nombreDimension = @"{ [Dim Empleado].[Dim Empleado Nombre].CHILDREN } ";
                    break;
                default: //Dimension por Default
                    nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].CHILDREN } ";
                    break;
            }

            string WITH = @"WITH
	            SET [TopVentas] AS
	            NONEMPTY(
		            ORDER(
			            STRTOSET(@Dimension),
			            [Measures].[Fact Ventas Netas], BDESC
		            )
	            )";

            string COLUMNS = @"NON EMPTY
                {
	                [Measures].[Fact Ventas Netas]
                }
                ON COLUMNS,";

            string ROWS = @"NON EMPTY
                {
	                HEAD([TopVentas],5)
                }
                ON ROWS";

            string CUBO_NAME = @"[DWH Northwind]";



            string MDXQuery = WITH + 
                              @"SELECT " + 
                              COLUMNS + 
                              ROWS + 
                              " FROM " + CUBO_NAME;

            Dictionary<string, decimal> structure = new Dictionary<string, decimal>();
            List<string> name = new List<string>();
            List<decimal> value = new List<decimal>();

            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    cmd.Parameters.Add(new AdomdParameter("Dimension", nombreDimension));
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            //structure.Add(dr.GetString(0), dr.GetDecimal(1));
                            name.Add(dr.GetString(0));
                            value.Add(dr.GetDecimal(1));
                            //Debug.Write(structure);
                        }
                        dr.Close();
                    }
                }
            }
            var data = (name, value);
            return Request.CreateResponse(HttpStatusCode.OK, data);
        }

        [HttpPost]
        [Route("SerieHistorica/{dimension}/{filtro}")]
        public HttpResponseMessage SerieHistorica(string dimension, string filtro)
        {
            //Se puso en filtro 0 para no tomarlo en cuenta y traer todas las de la categoría, sin embargo en el MDX no funciona, se trae todas las ventas
            string nombreDimension = @"";
            if (filtro == "0")
            {
                switch (dimension)
                {
                    case "1": // Clientes
                        nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[QUICK-Stop], [Dim Cliente].[Dim Cliente Nombre].&[Ernst Handel], [Dim Cliente].[Dim Cliente Nombre].&[Save-a-lot Markets], [Dim Cliente].[Dim Cliente Nombre].&[Rattlesnake Canyon Grocery], [Dim Cliente].[Dim Cliente Nombre].&[Hungry Owl All-Night Grocers] } ";
                        break;
                    case "2": // Productos
                        nombreDimension = @"{ [Dim Producto].[Dim Producto Nombre].&[Côte de Blaye], [Dim Producto].[Dim Producto Nombre].&[Thüringer Rostbratwurst], [Dim Producto].[Dim Producto Nombre].&[Raclette Courdavault], [Dim Producto].[Dim Producto Nombre].&[Tarte au sucre], [Dim Producto].[Dim Producto Nombre].&[Camembert Pierrot] } ";
                        break;
                    case "3": // Categorías
                        nombreDimension = @"{ [Dim Producto].[Dim Producto Categoria].&[Beverages], [Dim Producto].[Dim Producto Categoria].&[Dairy Products], [Dim Producto].[Dim Producto Categoria].&[Confections], [Dim Producto].[Dim Producto Categoria].&[Meat/Poultry], [Dim Producto].[Dim Producto Categoria].&[Seafood] } ";
                        break;
                    case "4": // Empleados
                        nombreDimension = @"{ [Dim Empleado].[Dim Empleado Nombre].&[Margaret Peacock], [Dim Empleado].[Dim Empleado Nombre].&[Janet Leverling], [Dim Empleado].[Dim Empleado Nombre].&[Nancy Davolio], [Dim Empleado].[Dim Empleado Nombre].&[Andrew Fuller],  [Dim Empleado].[Dim Empleado Nombre].&[Laura Callahan] } ";
                        break;
                    default: //Dimension por Default
                        nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[QUICK-Stop], [Dim Cliente].[Dim Cliente Nombre].&[Ernst Handel], [Dim Cliente].[Dim Cliente Nombre].&[Save-a-lot Markets], [Dim Cliente].[Dim Cliente Nombre].&[Rattlesnake Canyon Grocery], [Dim Cliente].[Dim Cliente Nombre].&[Hungry Owl All-Night Grocers] } ";
                        break;
                }
            }
            else
            {
                if (filtro == "Meat")
                {
                    filtro = "Meat/Poultry";
                }
                switch (dimension)
                {
                    case "1": // Clientes
                        nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[" + filtro + "] } ";
                        break;
                    case "2": // Productos
                        nombreDimension = @"{ [Dim Producto].[Dim Producto Nombre].&[" + filtro + "] } ";
                        break;
                    case "3": // Categorías
                        nombreDimension = @"{ [Dim Producto].[Dim Producto Categoria].&[" + filtro + "] } ";
                        break;
                    case "4": // Empleados
                        nombreDimension = @"{ [Dim Empleado].[Dim Empleado Nombre].&[" + filtro + "] } ";
                        break;
                    default: //Dimension por Default
                        nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[" + filtro + "] } ";
                        break;
                }
            }

            

            string COLUMNS = @"NON EMPTY
                {
	                [Measures].[Fact Ventas Netas]
                }
                ON COLUMNS,";

            string ROWS = @"NON EMPTY
                {
	                [Dim Tiempo].[Dim Tiempo Año].CHILDREN
                }
                *
                {
	                [Dim Tiempo].[Dim Tiempo Mes Siglas].CHILDREN
                }
                *
                {
                    STRTOSET(@Dimension)
                }


                ON ROWS";

            string CUBO_NAME = @"[DWH Northwind]";

            string MDXQuery = @"SELECT " +
                              COLUMNS +
                              ROWS +
                              " FROM " + CUBO_NAME;

            Dictionary<string, decimal> structure = new Dictionary<string, decimal>();
            List<string> year = new List<string>();
            List<string> month = new List<string>();
            List<decimal> value = new List<decimal>();
            List<string> filter = new List<string>();

            dynamic data;
            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    cmd.Parameters.Add(new AdomdParameter("Dimension", nombreDimension));
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {                           
                                year.Add(dr.GetString(0));
                                month.Add(dr.GetString(1));
                                filter.Add(dr.GetString(2));
                                value.Add(Math.Round(dr.GetDecimal(3)));
                            
                        }
                        dr.Close();
                    }
                }
                data = new { years = year, months = month, filters = filter, values = value };
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, (object)data);
        }

        [HttpPost]
        [Route("SerieHistoricaPost")]
        public HttpResponseMessage SerieHistoricaPost(Filtro objFiltro)
        {
            //Debug.Write(objFiltro);
            //return Request.CreateResponse(HttpStatusCode.OK, objFiltro);

            //Se puso en filtro 0 para no tomarlo en cuenta y traer todas las de la categoría, sin embargo en el MDX no funciona, se trae todas las ventas
            string nombreDimension = @"";
            if (objFiltro.Item == "0")
            {
                switch (objFiltro.Dimension)
                {
                    case "1": // Clientes
                        nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[QUICK-Stop], [Dim Cliente].[Dim Cliente Nombre].&[Ernst Handel], [Dim Cliente].[Dim Cliente Nombre].&[Save-a-lot Markets], [Dim Cliente].[Dim Cliente Nombre].&[Rattlesnake Canyon Grocery], [Dim Cliente].[Dim Cliente Nombre].&[Hungry Owl All-Night Grocers] } ";
                        break;
                    case "2": // Productos
                        nombreDimension = @"{ [Dim Producto].[Dim Producto Nombre].&[Côte de Blaye], [Dim Producto].[Dim Producto Nombre].&[Thüringer Rostbratwurst], [Dim Producto].[Dim Producto Nombre].&[Raclette Courdavault], [Dim Producto].[Dim Producto Nombre].&[Tarte au sucre], [Dim Producto].[Dim Producto Nombre].&[Camembert Pierrot] } ";
                        break;
                    case "3": // Categorías
                        nombreDimension = @"{ [Dim Producto].[Dim Producto Categoria].&[Beverages], [Dim Producto].[Dim Producto Categoria].&[Dairy Products], [Dim Producto].[Dim Producto Categoria].&[Confections], [Dim Producto].[Dim Producto Categoria].&[Meat/Poultry], [Dim Producto].[Dim Producto Categoria].&[Seafood] } ";
                        break;
                    case "4": // Empleados
                        nombreDimension = @"{ [Dim Empleado].[Dim Empleado Nombre].&[Margaret Peacock], [Dim Empleado].[Dim Empleado Nombre].&[Janet Leverling], [Dim Empleado].[Dim Empleado Nombre].&[Nancy Davolio], [Dim Empleado].[Dim Empleado Nombre].&[Andrew Fuller],  [Dim Empleado].[Dim Empleado Nombre].&[Laura Callahan] } ";
                        break;
                    default: //Dimension por Default
                        nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[QUICK-Stop], [Dim Cliente].[Dim Cliente Nombre].&[Ernst Handel], [Dim Cliente].[Dim Cliente Nombre].&[Save-a-lot Markets], [Dim Cliente].[Dim Cliente Nombre].&[Rattlesnake Canyon Grocery], [Dim Cliente].[Dim Cliente Nombre].&[Hungry Owl All-Night Grocers] } ";
                        break;
                }
            }
            else
            {
                if (objFiltro.Item == "Meat")
                {
                    objFiltro.Item = "Meat/Poultry";
                }
                switch (objFiltro.Dimension)
                {
                    case "1": // Clientes
                        nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[" + objFiltro.Item + "] } ";
                        break;
                    case "2": // Productos
                        nombreDimension = @"{ [Dim Producto].[Dim Producto Nombre].&[" + objFiltro.Item + "] } ";
                        break;
                    case "3": // Categorías
                        nombreDimension = @"{ [Dim Producto].[Dim Producto Categoria].&[" + objFiltro.Item + "] } ";
                        break;
                    case "4": // Empleados
                        nombreDimension = @"{ [Dim Empleado].[Dim Empleado Nombre].&[" + objFiltro.Item + "] } ";
                        break;
                    default: //Dimension por Default
                        nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[" + objFiltro.Item + "] } ";
                        break;
                }
            }



            string COLUMNS = @"NON EMPTY
                {
	                [Measures].[Fact Ventas Netas]
                }
                ON COLUMNS,";

            string ROWS = @"NON EMPTY
                {
	                [Dim Tiempo].[Dim Tiempo Año].CHILDREN
                }
                *
                {
	                [Dim Tiempo].[Dim Tiempo Mes Siglas].CHILDREN
                }
                *
                {
                    STRTOSET(@Dimension)
                }


                ON ROWS";

            string CUBO_NAME = @"[DWH Northwind]";

            string MDXQuery = @"SELECT " +
                              COLUMNS +
                              ROWS +
                              " FROM " + CUBO_NAME;

            Dictionary<string, decimal> structure = new Dictionary<string, decimal>();
            List<string> year = new List<string>();
            List<string> month = new List<string>();
            List<decimal> value = new List<decimal>();
            List<string> filter = new List<string>();

            dynamic data;
            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    cmd.Parameters.Add(new AdomdParameter("Dimension", nombreDimension));
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            year.Add(dr.GetString(0));
                            month.Add(dr.GetString(1));
                            filter.Add(dr.GetString(2));
                            value.Add(Math.Round(dr.GetDecimal(3)));

                        }
                        dr.Close();
                    }
                }
                data = new { years = year, months = month, filters = filter, values = value };
            }

            return Request.CreateResponse(HttpStatusCode.OK, (object)data);
        }

        [HttpPost]
        [Route("Combos")]
        public HttpResponseMessage Combos(Filtro objFiltro)
        {
            string nombreDimension = @"";

            switch (objFiltro.Dimension)
            {
                case "1": // Clientes
                    nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].CHILDREN } ";
                    break;
                case "2": // Productos
                    nombreDimension = @"{ [Dim Producto].[Dim Producto Nombre].CHILDREN } ";
                    break;
                case "3": // Categorías
                    nombreDimension = @"{ [Dim Producto].[Dim Producto Categoria].CHILDREN } ";
                    break;
                case "4": // Empleados
                    nombreDimension = @"{ [Dim Empleado].[Dim Empleado Nombre].CHILDREN } ";
                    break;
            }

            string COLUMNS = @"NON EMPTY
                {
	                [Measures].[Fact Ventas Netas]
                }
                ON COLUMNS,";

            string ROWS = @"NON EMPTY
                {
                    STRTOSET(@Dimension)
                }


                ON ROWS";

            string CUBO_NAME = @"[DWH Northwind]";

            string MDXQuery = @"SELECT " +
                              COLUMNS +
                              ROWS +
                              " FROM " + CUBO_NAME;

            Dictionary<int, string> structure = new Dictionary<int, string>();
            List<int> consecutivo = new List<int>();
            List<string> valor = new List<string>();
            var contador = 1;

            dynamic data;
            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    cmd.Parameters.Add(new AdomdParameter("Dimension", nombreDimension));
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            structure.Add(contador, dr.GetString(0));
                            consecutivo.Add(contador);
                            valor.Add(dr.GetString(0));

                            contador += 1;
                        }
                        dr.Close();
                    }
                }
                //data = new { id = consecutivo, itemName = valor };
            }

            //return Request.CreateResponse(HttpStatusCode.OK, (object)data);
            return Request.CreateResponse(HttpStatusCode.OK, structure);

        }

        [HttpPost]
        [Route("TablaComparativa")]
        public HttpResponseMessage TablaComparativa(Filtro objFiltro)
        {
            string nombreDimension = @"";

            switch (objFiltro.Dimension)
            {
                case "1": // Clientes
                    nombreDimension = @"{ [Dim Cliente].[Dim Cliente Nombre].&[" + objFiltro.Item + "] } ";
                    break;
                case "2": // Productos
                    nombreDimension = @"{ [Dim Producto].[Dim Producto Nombre].&[" + objFiltro.Item + "] } ";
                    break;
                case "3": // Categorías
                    nombreDimension = @"{ [Dim Producto].[Dim Producto Categoria].&[" + objFiltro.Item + "] } ";
                    break;
                case "4": // Empleados
                    nombreDimension = @"{ [Dim Empleado].[Dim Empleado Nombre].&[" + objFiltro.Item + "] } ";
                    break;
            }

            string WITH = @"WITH
	            SET [TopVentas] AS
	            NONEMPTY(
		            ORDER(
			            STRTOSET(@Dimension),
			            [Measures].[Fact Ventas Netas], BDESC
		            )
	            )";

            string COLUMNS = @"NON EMPTY
                {
	                [Measures].[Fact Ventas Netas]
                }
                ON COLUMNS,";

            string ROWS = @"NON EMPTY
                {
                    (
		                    [Dim Tiempo].[Dim Tiempo Mes Siglas].CHILDREN,
		                    [Dim Tiempo].[Dim Tiempo Año].CHILDREN
	                )
                }
                *
                {
                HEAD([TopVentas],1000)
                }
                ON ROWS";

            string CUBO_NAME = @"[DWH Northwind]";



            string MDXQuery = WITH +
                              @"SELECT " +
                              COLUMNS +
                              ROWS +
                              " FROM " + CUBO_NAME;

            List<string> fecha = new List<string>();
            List<decimal> valor = new List<decimal>();

            dynamic data;
            using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            {
                cnn.Open();
                using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
                {
                    cmd.Parameters.Add(new AdomdParameter("Dimension", nombreDimension));
                    using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (dr.Read())
                        {
                            fecha.Add(dr.GetString(0) + "-" + dr.GetString(1));
                            valor.Add(Math.Round(dr.GetDecimal(3)));
                        }
                        dr.Close();
                    }
                }
                data = new { fecha = fecha, valor = valor };
            }

            return Request.CreateResponse(HttpStatusCode.OK, (object)data);

        }


            //[HttpPost]
            //[Route("TablaComparativa")]
            //public HttpResponseMessage TablaComparativa(Filtro objFiltro)
            //{
            //    string nombreDimension = @"";

            //    string[] datos = objFiltro.Item.Split('|');

            //    for (int i = 0; i < datos.Length; i++)
            //    {
            //        if (i == 0)
            //        {
            //            switch (objFiltro.Dimension)
            //            {
            //                case "1": // Clientes
            //                    nombreDimension += @"[Dim Cliente].[Dim Cliente Nombre].&[" + datos[i] + "] ";
            //                    break;
            //                case "2": // Productos
            //                    nombreDimension += @"[Dim Producto].[Dim Producto Nombre].&[" + datos[i] + "] ";
            //                    break;
            //                case "3": // Categorías
            //                    nombreDimension += @"[Dim Producto].[Dim Producto Categoria].&[" + datos[i] + "] ";
            //                    break;
            //                case "4": // Empleados
            //                    nombreDimension += @"[Dim Empleado].[Dim Empleado Nombre].&[" + datos[i] + "] ";
            //                    break;
            //            }

            //        }
            //        else
            //        {
            //            switch (objFiltro.Dimension)
            //            {
            //                case "1": // Clientes
            //                    nombreDimension += @", [Dim Cliente].[Dim Cliente Nombre].&[" + datos[i] + "] ";
            //                    break;
            //                case "2": // Productos
            //                    nombreDimension += @", [Dim Producto].[Dim Producto Nombre].&[" + datos[i] + "] ";
            //                    break;
            //                case "3": // Categorías
            //                    nombreDimension += @", [Dim Producto].[Dim Producto Categoria].&[" + datos[i] + "] ";
            //                    break;
            //                case "4": // Empleados
            //                    nombreDimension += @", [Dim Empleado].[Dim Empleado Nombre].&[" + datos[i] + "] ";
            //                    break;
            //            }

            //        }
            //        //switch (objFiltro.Dimension)
            //        //{
            //        //    case "1": // Clientes
            //        //        nombreDimension += @"[Dim Cliente].[Dim Cliente Nombre].&[" + datos[i] + "] ";
            //        //        break;
            //        //    case "2": // Productos
            //        //        nombreDimension += @"[Dim Producto].[Dim Producto Nombre].&[" + datos[i] + "] ";
            //        //        break;
            //        //    case "3": // Categorías
            //        //        nombreDimension += @"[Dim Producto].[Dim Producto Categoria].&[" + datos[i] + "] ";
            //        //        break;
            //        //    case "4": // Empleados
            //        //        nombreDimension += @"[Dim Empleado].[Dim Empleado Nombre].&[" + datos[i] + "] ";
            //        //        break;
            //        //}

            //        ////Si es el último elemento, ya no se concatena la ,
            //        //if (i != (datos.Length -1))
            //        //{
            //        //    nombreDimension += @", ";
            //        //}
            //        //else
            //        //{
            //        //    nombreDimension += @" ";
            //        //}
            //    }

            //    string REPLACE = @"WITH MEMBER [Measures].[Fact Ventas Netas Formatted]  AS
            //                 IIF( ISEMPTY([Measures].[Fact Ventas Netas] )  ,'0',[Measures].[Fact Ventas Netas] )";

            //    string COLUMNS = @"NONEMPTY
            //                    (    
            //                     [Measures].[Fact Ventas Netas Formatted] 	 
            //                    )
            //                    *
            //                    { ";

            //    string FILTERS = @"
            //                     }
            //                    ON COLUMNS,";

            //    string ROWS = @"NON EMPTY
            //        {
            //            [Dim Tiempo].[Dim Tiempo Mes Siglas].CHILDREN
            //        }
            //        ON ROWS";

            //    string CUBO_NAME = @"[DWH Northwind]";

            //    string MDXQuery = REPLACE +
            //                      @"SELECT " +
            //                      COLUMNS +
            //                      nombreDimension +
            //                      FILTERS +
            //                      ROWS +
            //                      " FROM " + CUBO_NAME;

            //    Dictionary<int, string> structure = new Dictionary<int, string>();
            //    List<decimal> valor = new List<decimal>();
            //    List<string> fecha = new List<string>();
            //    List<List<decimal>> listaDeLista = new List<List<decimal>>();

            //    var contador = 0;

            //    dynamic data;
            //    using (AdomdConnection cnn = new AdomdConnection(ConfigurationManager.ConnectionStrings["CuboNorthwind"].ConnectionString))
            //    {
            //        cnn.Open();
            //        using (AdomdCommand cmd = new AdomdCommand(MDXQuery, cnn))
            //        {
            //            //cmd.Parameters.Add(new AdomdParameter("Dimension", nombreDimension));
            //            using (AdomdDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            //            {
            //                while (dr.Read())
            //                {
            //                    for (int i = 0; i < dr.FieldCount; i++)
            //                    {
            //                        if (i == 0)
            //                        {
            //                            fecha.Add(dr.GetString(i));
            //                        }
            //                        else if (i == (dr.FieldCount - 1))
            //                        {
            //                            listaDeLista.Add(valor);
            //                        }
            //                        else
            //                        {
            //                            valor.Add(Math.Round(dr.GetDecimal(i))); 
            //                        }
            //                    }

            //                    //var columnas = dr.FieldCount / fecha.Count;

            //                    //for (int i = 1; i < columnas - 1; i++)
            //                    //{
            //                    //    valor.Add(Math.Round(dr.GetDecimal(i)));
            //                    //    listaDeLista.Add(valor);
            //                    //}
            //                    //if (contador == 0)
            //                    //{
            //                    //    fecha.Add(dr.GetString(0));
            //                    //}
            //                    //else if (contador < (dr.FieldCount))
            //                    //{
            //                    //    valor.Add(Math.Round(dr.GetDecimal(contador)));
            //                    //}

            //                    //valor.Add(dr.GetString(0));

            //                    contador += 1;
            //                }
            //                dr.Close();
            //            }
            //        }
            //        data = new { fechas = fecha, valores = listaDeLista };
            //    }

            //    return Request.CreateResponse(HttpStatusCode.OK, (object)data);
            //    //return Request.CreateResponse(HttpStatusCode.OK, structure);
            //}
        }

    public class Filtro
    {
        public string Dimension { get; set; }
        public string Item { get; set; }
    }
}
