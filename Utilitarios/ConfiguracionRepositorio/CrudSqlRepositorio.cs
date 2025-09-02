namespace Utilitarios.ConfiguracionRepositorio
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Newtonsoft.Json;
    using System.Data;
    using System.Linq.Expressions;
    using Utilitarios.Entidades;
    using Utilitarios.Interfaces.ConfiguracionRepositorio;

    public class CrudSqlRepositorio<T> : ICrudSqlRepositorio<T> where T : EntidadBase
    {
        #region Variables

        /// <summary>Tipado de la entidad.</summary>
        protected readonly DbSet<T> _entidad;
        protected readonly DbContext _entidadContext;

        #endregion

        #region Constructor

        ///<summary>Inicialización de la clase.</summary>
        public CrudSqlRepositorio(DbContext options)
        {
            _entidad = options.Set<T>();
            _entidadContext = options;
        }

        #endregion

        #region Consulta

        /// <summary>Método para consultar un registro de la entidad requerida.</summary>
        /// <returns>Registro de la entidad encontrada.</returns>
        public async Task<T> ConsultarPorId(byte id)
        {
            try
            {
                return await _entidad.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de consultar el registro.", ex);
            }
        }

        /// <summary>Método para consultar un registro de la entidad requerida.</summary>
        /// <returns>Registro de la entidad encontrada.</returns>
        public async Task<T> ConsultarPorId(short id)
        {
            try
            {
                return await _entidad.FindAsync(id);
            }
            catch (Exception ex)
            {

                throw new ArgumentException("Se presento un error al momento de consultar el registro.", ex);
            }
        }

        /// <summary>Método para consultar un registro de la entidad requerida.</summary>
        /// <returns>Registro de la entidad encontrada.</returns>
        public async Task<T> ConsultarPorId(int id)
        {
            try
            {
                return await _entidad.FindAsync(id);
            }
            catch (Exception ex)
            {

                throw new ArgumentException("Se presento un error al momento de consultar el registro.", ex);
            }
        }

        /// <summary>Método para consultar un registro de la entidad requerida.</summary>
        /// <returns>Registro de la entidad encontrada.</returns>
        public async Task<T> ConsultarPorId(long id)
        {
            try
            {
                return await _entidad.FindAsync(id);
            }
            catch (Exception ex)
            {

                throw new ArgumentException("Se presento un error al momento de consultar el registro.", ex);
            }
        }

        /// <summary>Método para consultar un registro según los parametros recibidos en objBusqueda.</summary>
        /// <param name="objBusqueda">Contiene los filtros de busqueda para retornar el registro encontrado.</param>
        /// <returns>Objeto de tipo entidad encontrado.</returns>
        public async Task<T> ConsultarObjeto(Expression<Func<T, bool>> objBusqueda)
        {
            return await _entidad.FirstOrDefaultAsync(objBusqueda);
        }

        /// <summary>Método para consultar todos los registros de la entidad requerida.</summary>
        /// <returns>Lista de registros de la entidad encontradas.</returns>
        public IEnumerable<T> ConsultarTodos()
        {
            try
            {
                return _entidad.AsQueryable();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de consultar los registros.", ex);
            }
        }

        /// <summary>Método para consultar todos los registros de la entidad requerida.</summary>
        /// <returns>Lista de registros de la entidad encontradas.</returns>
        public List<T> ConsultarTodosLista()
        {
            try
            {
                return _entidad.AsQueryable().ToList();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de consultar los registros.", ex);
            }
        }

        /// <summary>Método para consultar todos los registros de la entidad requerida.</summary>
        /// <returns>Lista de registros de la entidad encontradas.</returns>
        public IEnumerable<T> ConsultarTodosFiltroQuery(Expression<Func<T, bool>> objBusqueda)
        {
            // Para un llamado asincronico utilizar el método ConsultarLista.
            return _entidad.Where(objBusqueda);
        }

        /// <summary>Método para consultar una lista según los parametros recibidos en objBusqueda.</summary>
        /// <param name="objBusqueda">Contiene los filtros de busqueda para retornar la lista de registros encontrados.</param>
        /// <returns>Lista de todos los registros encontrados.</returns>
        public async Task<IEnumerable<T>> ConsultarLista(Expression<Func<T, bool>> objBusqueda)
        {
            return await _entidad.Where(objBusqueda).ToListAsync();
        }

        /// <summary>Metodo para consultar procedimiento almacenado multiparametro enviando un tipo T entidad de resultado.</summary>
        /// <param name="procedure">Nombre del procedimiento.</param>
        /// <param name="variables">Nombre parametro.</param>
        /// <param name="parameters">Valor parametro.</param>
        /// <returns>Lista de registro segun entidad T transferida.</returns>
        public async Task<IEnumerable<T>> ConsultarStoreProcedureMultiParametro<T>(string procedure, object[] variables, object[] parameters)
        {
            var dataSetResultado = new DataSet();
            List<T> lista = new();

            var connection = _entidadContext.Database.GetDbConnection();
            try
            {
                await _entidadContext.Database.OpenConnectionAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = procedure;

                    if (variables != null)
                    {
                        for (int i = 0; i < variables.Length; i++)
                        {
                            var p = command.CreateParameter();
                            p.ParameterName = variables[i].ToString();
                            p.Value = parameters[i] ?? DBNull.Value;
                            command.Parameters.Add(p);
                        }
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        do
                        {
                            var table = new DataTable();
                            table.Load(reader);
                            dataSetResultado.Tables.Add(table);
                        } while (!reader.IsClosed);
                    }
                }
            }
            finally
            {
                _entidadContext.Database.CloseConnection(); // <-- MUY IMPORTANTE
            }

            foreach (DataTable dt in dataSetResultado.Tables)
            {
                var columnNames = dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                var properties = typeof(T).GetProperties();

                var resultado = dt.AsEnumerable().Select(row =>
                {
                    var objT = Activator.CreateInstance<T>();

                    foreach (var prop in properties)
                    {
                        if (columnNames.Contains(prop.Name))
                        {
                            object value = row[prop.Name];
                            prop.SetValue(objT, value == DBNull.Value ? null : value);
                        }
                    }

                    return objT;
                }).ToList();

                lista.AddRange(resultado);
            }

            return lista;
        }


        /// <summary>Metodo para consultar procedimiento almacenado multiparametro.</summary>
        /// <param name="procedure">Nombre del procedimiento.</param>
        /// <param name="variables">Nombre parametro. </param>
        /// <param name="parameters">Valor parametro. </param>
        /// <returns>Lista de registro segun entidad T transferida.</returns>
        public async Task<IEnumerable<T>> ConsultarStoreProcedureMultiParametro(string procedure, object[] variables, object[] parameters)
        {
            var dataSetResultado = new DataSet();
            List<T> lista = new List<T>();

            using (var cnn = _entidadContext.Database.GetDbConnection())
            {
                var cmm = cnn.CreateCommand();
                cmm.CommandType = CommandType.StoredProcedure;
                cmm.CommandText = procedure;
                cmm.Connection = cnn;

                if (variables != null)
                {
                    for (int i = 0; i < variables.Length; i++)
                    {
                        var p = cmm.CreateParameter();
                        p.ParameterName = variables[i].ToString();
                        p.Value = parameters[i]?.ToString();
                        cmm.Parameters.Add(p);
                    }
                }

                cnn.Open();
                var reader = await cmm.ExecuteReaderAsync();

                do
                {
                    var tb = new DataTable();
                    tb.Load(reader);
                    dataSetResultado.Tables.Add(tb);
                } while (!reader.IsClosed);
            }


            foreach (DataTable dt in dataSetResultado.Tables)
            {
                var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();

                var properties = typeof(T).GetProperties();

                var resultado = dt.AsEnumerable().Select(row =>
                {
                    var objT = Activator.CreateInstance<T>();

                    foreach (var pro in properties)
                    {
                        if (columnNames.Contains(pro.Name))
                            pro.SetValue(objT, row[pro.Name] == DBNull.Value ? null : row[pro.Name]);
                    }

                    return objT;
                }).ToList();

                lista.AddRange(resultado);
            }

            return lista;
        }

        /// <summary>Método para ejecutar un store procedure</summary>
        /// <param name="procedure">Nombre del procedimiento</param>
        /// <param name="variables">Nombre parametro </param>
        /// <param name="parametros">Valor parametro</param>
        /// <returns>json retornado por el store procedure</returns>
        public async Task<string> ConsultarStoreProcedure(string procedure, object[] variables, object[] parametros)
        {
            var dataSetResultado = new DataSet();

            using (var cnn = _entidadContext.Database.GetDbConnection())
            {
                var cmm = cnn.CreateCommand();
                cmm.CommandType = CommandType.StoredProcedure;
                cmm.CommandText = procedure;
                cmm.CommandTimeout = 300;
                cmm.Connection = cnn;

                if (variables != null)
                {
                    for (int i = 0; i < variables.Length; i++)
                    {
                        var p = cmm.CreateParameter();
                        p.ParameterName = variables[i].ToString();
                        p.Value = parametros[i]?.ToString();
                        cmm.Parameters.Add(p);
                    }
                }

                cnn.Open();
                var reader = await cmm.ExecuteReaderAsync();

                do
                {
                    var tb = new DataTable();
                    tb.Load(reader);
                    dataSetResultado.Tables.Add(tb);
                } while (!reader.IsClosed);
            }

            // Aquí se asume que hay al menos una tabla en el DataSet
            var resultTable = dataSetResultado.Tables[0];

            // Convertir la tabla a JSON
            var jsonResult = JsonConvert.SerializeObject(resultTable, Formatting.Indented);

            return jsonResult;
        }

        #endregion

        #region Adicionar

        /// <summary>Método para adicionar un registro de la entidad requerida.</summary>
        /// <param name="objAdicionar">Entidad que será adicionada.</param>
        /// <returns>Entidad con el identifador del registro adicionado.</returns>
        public async Task Adicionar(T objAdicionar)
        {
            try
            {
                await _entidad.AddAsync(objAdicionar);
            }
            catch (Exception ex)
            {

                throw new ArgumentException("Se presento un error al momento de adicionar el registro.", ex);
            }
        }

        /// <summary>Método para adicionar registros de la entidad de manera masiva.</summary>
        /// <param name="lstAdicionar">Lista de entidad que será adicionado.</param>
        /// <returns>true/false si fueron adicionados.</returns>
        public async Task AdicionarMasivo(List<T> lstAdicionar)
        {
            try
            {
                await _entidad.AddRangeAsync(lstAdicionar);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de adicionar los registros.", ex);
            }
        }

        #endregion

        #region Actualizar

        /// <summary>Método para actualizar un registro en la entidad.</summary>
        /// <param name="objActualizar">Entidad que será actualizada.</param>
        /// <returns>Verdadero en caso que la entidad se actualice correctamente o falso en caso contrario.</returns>
        public void Actualizar(T objActualizar)
        {
            try
            {
                _entidad.Update(objActualizar);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de actualizar el registro.", ex);
            }
        }

        /// <summary>Método para actualizar un registro en la entidad.</summary>
        /// <param name="objActualizar">Entidad que será actualizada.</param>
        /// <returns>Verdadero en caso que la entidad se actualice correctamente o falso en caso contrario.</returns>
        public async Task ActualizarAsync(T objActualizar)
        {
            try
            {
                EntityEntry entityEntry = _entidadContext.Entry(objActualizar);
                entityEntry.State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de actualizar el registro.", ex);
            }
        }

        #endregion

        #region Eliminar

        /// <summary>Método para eliminar un registro en la entidad.</summary>
        /// <param name="id">Identificador de la entidad a eliminar.</param>
        /// <returns>Verdadero en caso que la entidad se elimine correctamente o falso en caso contrario.</returns>
        public async Task Eliminar(int id)
        {
            try
            {
                T entidad = await ConsultarPorId(id);
                _entidad.Remove(entidad);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de eliminar el registro.", ex);
            }
        }

        /// <summary>Método para eliminar un registro en la entidad.</summary>
        /// <param name="id">Identificador de la entidad a eliminar.</param>
        /// <returns>Verdadero en caso que la entidad se elimine correctamente o falso en caso contrario.</returns>
        public async Task Eliminar(long id)
        {
            try
            {
                T entidad = await ConsultarPorId(id);
                _entidad.Remove(entidad);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de eliminar el registro.", ex);
            }
        }

        /// <summary>Método para eliminar un grupo de registros</summary>
        /// <param name="objEliminar">Información de la entidad a eliminar.</param>
        /// <returns>Verdadero en caso que la entidad se elimine correctamente o falso en caso contrario.</returns>
        public async Task EliminarAsync(T objEliminar)
        {
            try
            {
                _entidad.Remove(objEliminar);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de eliminar el registro.", ex);
            }

        }

        /// <summary>Método para eliminar Masivo en la entidad.</summary>
        /// <param name="objEliminar">Información de la entidad a eliminar.</param>
        /// <returns>Verdadero en caso que la entidad se elimine correctamente o falso en caso contrario.</returns>
        public async Task EliminarMasivoAsync(List<T> objEliminar)
        {
            try
            {
                _entidad.RemoveRange(objEliminar);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de eliminar el registro.", ex);
            }

        }

        /// <summary>Método para eliminar un registro en la entidad.</summary>
        /// <param name="objEliminar">Información de la entidad a eliminar.</param>
        /// <returns>Verdadero en caso que la entidad se elimine correctamente o falso en caso contrario.</returns>
        public void Eliminar(T objEliminar)
        {
            try
            {
                _entidad.Remove(objEliminar);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Se presento un error al momento de eliminar el registro.", ex);
            }

        }

        #endregion

    }
}