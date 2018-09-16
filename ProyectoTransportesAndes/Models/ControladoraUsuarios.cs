using Microsoft.Extensions.Options;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
using ProyectoTransportesAndes.Persistencia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoTransportesAndes.Models
{
    public class ControladoraUsuarios
    {

        #region Atributos

        private IOptions<AppSettingsMongo> _settings;
        private static ControladoraUsuarios _instance;

        #endregion

        #region Propiedades

        public static ControladoraUsuarios getInstance(IOptions<AppSettingsMongo> settings)
        {
            if (_instance == null)
            {
                _instance = new ControladoraUsuarios(settings);
            }
            return _instance;
        }
        private Hashtable UbicacionesClientes { get; set; }


        #endregion

        #region Constructores

        private ControladoraUsuarios(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            DBRepositoryMongo<Administrativo>.Iniciar(_settings);
            DBRepositoryMongo<Chofer>.Iniciar(_settings);
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
            DBRepositoryMongo<Peon>.Iniciar(_settings);
            UbicacionesClientes = new Hashtable();

        }

        #endregion

        #region Metodos

        /// <summary>
        /// DEVUELVE LOS ADMINISTRATIVOS DEL SISTEMA
        /// DEJA POR FUERA UN USUARIO "SUPER" PARA QUE NO PUEDA SER MODIFICADO
        /// </summary>
        /// <returns>LISTA DE ADMINISTRATIVOS</returns>
        public async Task<List<Administrativo>> getAdministrativos()
        {
            try
            {
                var items = await DBRepositoryMongo<Administrativo>.GetItemsAsync("Administrativos");
                List<Administrativo> aux = items.ToList();
                List<Administrativo> salida = new List<Administrativo>();
                Administrativo admin = null;

                foreach (Administrativo a in aux)
                {
                    admin = a.Desencriptar(a);
                    salida.Add(admin);
                }
                Administrativo eliminar = null;
                foreach (Administrativo a in salida)
                {
                    if (a.User.Equals("super"))
                    {
                        eliminar = a;
                    }
                }
                salida.Remove(eliminar);
                return salida;
            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error inesperado, intente de nuevo mas tarde");
            }
           
        }

        /// <summary>
        /// DEVUELVE EL ADMINISTRATIVO CON EL SOLICITADO
        /// </summary>
        /// <param name="id"></param>
        /// <returns>ADMINISTRATIVO</returns>
        public async Task<Administrativo>getAdministrativo(string id)
        {
            Administrativo administrativo = null;
            if (id == null)
            {
                throw new MensajeException("Id de usuario inexistente");
            }
            else
            {
                administrativo = await DBRepositoryMongo<Administrativo>.GetItemAsync(id, "Administrativos");
                administrativo = administrativo.Desencriptar(administrativo);
                return administrativo;
            }
        }

        /// <summary>
        /// DEVUELVE EL CHOFER CON EL ID SOLICITADO
        /// </summary>
        /// <param name="id"></param>
        /// <returns>CHOFER</returns>
        public async Task<Chofer> getChofer(string id)
        {
            Chofer usuario = null;
            if (id == null)
            {
                throw new MensajeException("Id de usuario inexistente");
            }
            else
            {
                usuario = await DBRepositoryMongo<Chofer>.GetItemAsync(id, "Choferes");
                usuario.Desencriptar(usuario);
                return usuario;
            }
        }

        /// <summary>
        /// DEVUELVE LA LISTA DE CHOFERES QUE HAY EN EL SISTEMA
        /// </summary>
        /// <returns>LISTA DE CHOFER</returns>
        public async Task<List<Chofer>> getChoferes()
        {
            try
            {
                var items = await DBRepositoryMongo<Chofer>.GetItemsAsync("Choferes");
                List<Chofer> salida = new List<Chofer>();
                foreach (Chofer c in items)
                {
                    Chofer aux = c.Desencriptar(c);
                    salida.Add(aux);
                }
                return salida;
            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error inesperado, intente de nuevo mas tarde");
            }
        }

        /// <summary>
        /// DEVUELVE UNA LISTA DE LOS CLIENTES QUE HAY EN EL SISTEMA
        /// </summary>
        /// <param name="id"></param>
        /// <returns>LISTA DE CLIENTE</returns>
        public async Task<Cliente> getCliente(string id)
        {
            Cliente cliente = null;
            if (id == null)
            {
                throw new MensajeException("Id de usuario inexistente");
            }
            else
            {
                cliente = await DBRepositoryMongo<Cliente>.GetItemAsync(id, "Clientes");
                cliente = cliente.Desencriptar(cliente);
                return cliente;
            }
        }

        /// <summary>
        /// DEVUELVE UNA LISTA DE LOS PEONES QUE HAY EN EL SISTEMA
        /// </summary>
        /// <returns>LISTA PEON</returns>
        public async Task<List<Peon>> getPeones()
        {
            try
            {
                var items = await DBRepositoryMongo<Peon>.GetItemsAsync("Peones");
                List<Peon> salida = new List<Peon>();
                foreach(Peon p in items)
                {
                    Peon peon = p.Desencriptar(p);
                    salida.Add(p);
                }
                return salida;
            }
            catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DEVUELVE EL PEON CON EL ID SOLICITADO
        /// </summary>
        /// <param name="idPeon"></param>
        /// <returns>PEON</returns>
        public async Task<Peon> getPeon(string idPeon)
        {
            try
            {
                Peon peon = await DBRepositoryMongo<Peon>.GetItemAsync(idPeon, "Peones");
                peon = peon.Desencriptar(peon);
                return peon;
            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DEVUELVE UNA LISTA DE CLIENTES QUE HAY EN EL SISTEMA
        /// </summary>
        /// <returns>LISTA DE CLIENTE</returns>
        public async Task<List<Cliente>> getClientes()
        {
            try
            {
                var items = await DBRepositoryMongo<Cliente>.GetItemsAsync("Clientes");
                List<Cliente> clientes = new List<Cliente>();
                foreach (Cliente c in items)
                {
                    Cliente aux = c.Desencriptar(c);
                    clientes.Add(aux);
                }
                return clientes;
            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception)
            {
                throw new MensajeException("Se produjo un error inesperado, intente de nuevo mas tarde");
            }
        }

        /// <summary>
        /// devuelve el administrativo o el cliente si lo puede validar o si no devuelve null
        /// </summary>
        /// <param name="administrativo"></param>
        /// <param name="pass"></param>
        /// <returns>el administrativo o cliente con las credenciales solicitadas</returns>
        public async Task<Usuario> Login(string administrativo, string pass)
        {
            MensajeException mensajeError = new MensajeException("Usuario y/o contraseña incorrecta");
            Usuario salida = null;
            try
            {
                Administrativo user = await DBRepositoryMongo<Administrativo>.Login(Seguridad.Encriptar(administrativo), "Administrativos");
                if (user == null)
                {
                    Cliente cliente = await DBRepositoryMongo<Cliente>.Login(Seguridad.Encriptar(administrativo), "Clientes");
                    if (cliente != null)
                    {
                        cliente = cliente.Desencriptar(cliente);
                        if (cliente.Password == pass)
                        {
                            salida = cliente; 
                        }
                        else
                        {
                            throw mensajeError;
                        }
                    }
                    else
                    {
                        throw new MensajeException("Usuario incorrecto");
                    }
                }
                else
                {
                    user = user.Desencriptar(user);
                    if (user.Password == pass)
                    {
                        salida = user;
                    }
                    else
                    {
                        throw mensajeError;
                    }
                }
                return salida;

            }catch(MensajeException msg)
            {
                throw msg;
            }
        }

        /// <summary>
        /// devuelve el chofer si lo puede validar o si no devuelve null
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="pass"></param>
        /// <returns>el chofer con las credenciales solicitadas</returns>
        public async Task<Chofer>LoginChofer(string usuario, string pass)
        {
            try
            {
                Chofer chofer = await DBRepositoryMongo<Chofer>.Login(Seguridad.Encriptar(usuario), "Choferes");
                if (chofer != null)
                {
                    chofer = chofer.Desencriptar(chofer);
                    if (chofer.Password == pass)
                    {
                        return chofer;
                    }
                    else
                    {
                        throw new MensajeException("Usuario y/o contraseña incorrecta");
                    }
                }
                else
                {
                    throw new MensajeException("Usuario y/o contraseña incorrecta");
                }
            }
            catch(MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// crea un usuario administrativo
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="administrador"></param>
        /// <returns>el administrativo creado</returns>
        public async Task<Usuario> CrearAdministrativo(Administrativo usuario, bool administrador)
        {
            try
            {
                Usuario salida = null;
                Usuario u = await DBRepositoryMongo<Usuario>.GetUsuario(Seguridad.Encriptar(usuario.User), "Administrativos");
                if (u != null)
                {
                    throw new MensajeException("El usuario ya existe");
                }
                else
                {
                    if (usuario.Ubicacion == null)
                    {
                        usuario.Ubicacion = new PosicionSatelital() { Latitud = "", Longitud = "" };
                    }
                    Administrativo nuevo = new Administrativo();
                    nuevo = usuario;
                    nuevo.Tipo = "Administrador";
                    if (administrador)
                    {
                        nuevo.Administrador = true;
                    }
                    else
                    {
                        nuevo.Administrador = false;
                    }
                    nuevo = nuevo.Encriptar(nuevo);
                    await DBRepositoryMongo<Usuario>.Create(nuevo, "Administrativos");
                    salida = nuevo;
                }
                return salida;
            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// elimna el administrativo pasado como parametro
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task EliminarAdministrativo(Usuario usuario, string id)
        {
            try
            {
                if(usuario !=null && id != null)
                {
                    await DBRepositoryMongo<Usuario>.DeleteAsync(id, "Administrativos");
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                }
              
            }
            catch(MensajeException msg)
            {
                throw msg;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }

        /// <summary>
        /// modifica un administrativo pasado como parametro
        /// </summary>
        /// <param name="administrativo"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task ModificarAdministrativo(Administrativo administrativo, string id) 
        {
            try
            {
                if (administrativo != null && id!=null)
                {
                    administrativo.Id = new ObjectId(id);
                    if (administrativo.Ubicacion == null)
                    {
                        administrativo.Ubicacion = new PosicionSatelital() { Latitud = "", Longitud = "" };
                    }
                    administrativo = administrativo.Encriptar(administrativo);
                    await DBRepositoryMongo<Usuario>.UpdateAsync(administrativo.Id, administrativo, "Administrativos");
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde");
                }
               
            }
            catch(MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

        /// <summary>
        /// ingresa un nuevo chofer al sistema y lo devuelve
        /// </summary>
        /// <param name="chofer"></param>
        /// <param name="libreta"></param>
        /// <returns>Chofer</returns>
        public async Task<Chofer> CrearChofer(Chofer chofer, LibretaDeConducir libreta)
        {
            Chofer salida = null;
            try
            {
                Chofer c = await DBRepositoryMongo<Chofer>.GetUsuario(chofer.User, "Choferes");
                if (c != null)
                {
                    throw new MensajeException("El chofer ya existe");
                }
                else
                {
                    salida = chofer;
                    salida.Tipo = "Chofer";
                    salida.Disponible = true;
                    salida.LibretaDeConducir = libreta;
                    salida.Leyenda = chofer.Numero + " - " + chofer.Nombre + " " + chofer.Apellido;
                    if (salida.Ubicacion == null)
                    {
                        salida.Ubicacion = new PosicionSatelital() { Latitud = "", Longitud = "" };
                    }
                    salida = salida.Encriptar(salida);
                    await DBRepositoryMongo<Chofer>.Create(salida, "Choferes");
                }
                return salida;
            }
            catch(MensajeException msg)
            {
                throw msg;
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// modifica el chofer pasado en la base. Se tienen en cuenta tambien los vehiculos en base y en memoria
        /// </summary>
        /// <param name="chofer"></param>
        /// <param name="idChofer"></param>
        /// <returns></returns>
        public async Task ModificarChofer(Chofer chofer, string idChofer)
        {
            try
            {
                if (chofer != null && idChofer != null)
                {
                    chofer.Id = new ObjectId(idChofer);
                    if (chofer.Ubicacion == null)
                    {
                        chofer.Ubicacion = new PosicionSatelital() { Latitud = "", Longitud = "" };
                    }
                    chofer = chofer.Encriptar(chofer);
                    await DBRepositoryMongo<Chofer>.UpdateAsync(chofer.Id, chofer, "Choferes");
                    Vehiculo v = await ControladoraVehiculos.getInstance(_settings).getVehiculoChofer(idChofer);
                    if (v != null)
                    {
                        v.Chofer = chofer.Encriptar(chofer);
                        await ControladoraVehiculos.getInstance(_settings).editarVehiculo(v, v.Id.ToString(), chofer.Id.ToString(), v.Tipo);
                    }
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde");
                }

            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// SE ELIMINA EL CHOFER Y SE ACTUALIZAN LOS VEHICULOS EN MEMORIA Y EN LA BD
        /// </summary>
        /// <param name="chofer"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task EliminarChofer(Chofer chofer, string id)
        {
            try
            {
                if (chofer != null && id != null)
                {
                    await DBRepositoryMongo<Usuario>.DeleteAsync(id, "Choferes");
                    var vehiculos = await ControladoraVehiculos.getInstance(_settings).getVehiculos();
                    List<Vehiculo> aux = vehiculos.ToList();
                    foreach (Vehiculo v in aux)
                    {
                        if (v.Chofer.Id.ToString().Equals(id))
                        {
                            v.Chofer = new Chofer();
                            await ControladoraVehiculos.getInstance(_settings).editarVehiculo(v, v.Id.ToString(), v.Chofer.Id.ToString(), v.Tipo); //  se acualiza el vehiculo en memoria y en la bd
                        }
                    }
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                }

            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Inserta un cliente encriptado en la base de datos
        /// </summary>
        /// <param name="cliente"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public async Task CrearCliente(Cliente cliente, TarjetaDeCredito tarjeta)
        {
            try
            {
                Cliente salida = null;
                Usuario usuario = await DBRepositoryMongo<Usuario>.GetUsuario(Seguridad.Encriptar(cliente.User), "Usuarios");
                Cliente cli = await DBRepositoryMongo<Cliente>.GetUsuario(Seguridad.Encriptar(cliente.User), "Clientes");
                Chofer chofer = await DBRepositoryMongo<Chofer>.GetUsuario(Seguridad.Encriptar(cliente.User), "Choferes");
                if (usuario == null && cli == null && chofer == null)
                {
                    cliente.Tipo = "Cliente";
                    cliente.Tarjeta = tarjeta;
                    if (cliente.RazonSocial != null)
                    {
                        cliente.Leyenda = cliente.RazonSocial;
                    }
                    else
                    {
                        cliente.Leyenda = cliente.Nombre + " " + cliente.Apellido;
                        cliente.RazonSocial = "";

                    }
                    if (cliente.Rut == null)
                    {
                        cliente.Rut = "";
                    }
                    if (cliente.Ubicacion == null)
                    {
                        cliente.Ubicacion = new PosicionSatelital() { Latitud = "", Longitud = "" };
                    }
                    if (string.IsNullOrEmpty(cliente.Tarjeta.fVencimiento))
                    {
                        cliente.Tarjeta.fVencimiento = "";
                    }
                    if (string.IsNullOrEmpty(cliente.Tarjeta.Numero))
                    {
                        cliente.Tarjeta.Numero = "";
                    }
                    salida = cliente.Encriptar(cliente);
                    await DBRepositoryMongo<Cliente>.Create(salida, "Clientes");
                }
                else
                {
                    throw new MensajeException("Ya existe un usuario con ese nick");
                }
                
            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// SE ELIMINA EL CLIENTE SELECCIONADO
        /// </summary>
        /// <param name="cliente"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task EliminarCliente(Cliente cliente, string id)
        {
            try
            {
                if (cliente != null && id != null)
                {
                    
                    await DBRepositoryMongo<Usuario>.DeleteAsync(id, "Clientes");
                    
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                }

            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// SE MODIFICA EL CLIENTE SELECCIONADO
        /// </summary>
        /// <param name="cliente"></param>
        /// <param name="id"></param>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        public async Task ModificarCliente(Cliente cliente, string id, TarjetaDeCredito tarjeta)
        {
            try
            {
                if (cliente != null && id != null)
                {
                    cliente.Id = new ObjectId(id);
                    cliente.Tarjeta = tarjeta;
                    cliente.Encriptar(cliente);
                    await DBRepositoryMongo<Cliente>.UpdateAsync(cliente.Id, cliente, "Clientes");
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde");
                }

            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// SE CREA UN NUEVO PEON EN LA BD
        /// </summary>
        /// <param name="peon"></param>
        /// <returns></returns>
        public async Task CrearPeon(Peon peon)
        {
            try
            {
                Peon salida = null;
                Peon p = await DBRepositoryMongo<Peon>.GetPeon(Seguridad.Encriptar(peon.Documento), "Peones");
                if (p == null)
                {
                    salida = peon;
                    salida = salida.Encriptar(salida);
                    await DBRepositoryMongo<Peon>.Create(salida, "Peones");
                }
                else
                {
                    throw new MensajeException("Ya existe el peon");
                }

            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// SE MODIFICA EL PEON SELECCIONADO EN LA BD
        /// </summary>
        /// <param name="peon"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task ModificarPeon(Peon peon, string id)
        {
            try
            {
                if (peon != null && id != null)
                {
                    peon.Id = new ObjectId(id);
                    peon.Encriptar(peon);
                    await DBRepositoryMongo<Peon>.UpdateAsync(peon.Id, peon, "Peones");
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado. Vuelva a intentarlo mas tarde");
                }

            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// SE ELIMINA EL PEON SELECCIONADO DE LA BD
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task EliminarPeon(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    await DBRepositoryMongo<Peon>.DeleteAsync(id, "Peones");
                }
                else
                {
                    throw new MensajeException("Ha ocurrido un error inesperado, vuelva a intentarlo mas tarde");
                }

            }
            catch (MensajeException msg)
            {
                throw msg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// guarda la posicion satelital del cliente con el id pasado como parametro en el hash en memoria
        /// </summary>
        /// <param name="idCliente"></param>
        /// <param name="latitud"></param>
        /// <param name="longitud"></param>
        /// <returns></returns>
        public PosicionSatelital guardarUbicacionCliente(string idCliente, string latitud, string longitud)
        {
            PosicionSatelital posicion = new PosicionSatelital(idCliente, latitud, longitud);
            if (UbicacionesClientes.Contains(idCliente))
            {
                UbicacionesClientes[idCliente] = posicion;
            }
            else
            {
                UbicacionesClientes.Add(posicion.Id, posicion);
            }
            return posicion;
        }

        /// <summary>
        /// obtiende del hash de posiciones de cliente la posicion con el id de cliente pasado como parametro
        /// </summary>
        /// <param name="idCliente"></param>
        /// <returns></returns>
        public PosicionSatelital obtenerUbicacionCliente(string idCliente)
        {
            return (PosicionSatelital)UbicacionesClientes[idCliente];
        }

        #endregion


    }
}
