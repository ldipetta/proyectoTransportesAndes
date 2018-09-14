using Microsoft.Extensions.Options;
using MongoDB.Bson;
using ProyectoTransportesAndes.Configuracion;
using ProyectoTransportesAndes.Exceptions;
using ProyectoTransportesAndes.Persistencia;
using System;
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
        #endregion

        #region Constructores
        private ControladoraUsuarios(IOptions<AppSettingsMongo> settings)
        {
            _settings = settings;
            DBRepositoryMongo<Usuario>.Iniciar(_settings);
            DBRepositoryMongo<Chofer>.Iniciar(_settings);
            DBRepositoryMongo<Cliente>.Iniciar(_settings);
            DBRepositoryMongo<Peon>.Iniciar(_settings);
        }
        #endregion

        #region Metodos
        //Devuelve una lista de usuarios de tipo pasado por parametro
        public async Task<List<Usuario>> getAdministrativos()
        {
            try
            {
                var items = await DBRepositoryMongo<Usuario>.GetItemsAsync("Usuarios");
                List<Usuario> aux = items.ToList();
                List<Usuario> salida = new List<Usuario>();
                Usuario usuario = null;

                foreach (Usuario u in aux)
                {
                    usuario = u.Desencriptar(u);
                    salida.Add(usuario);
                }
                Usuario eliminar = null;
                foreach(Usuario u in salida)
                {
                    if (u.User.Equals("super"))
                    {
                        eliminar = u;
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
        public async Task<Usuario>getAdministrativo(string id)
        {
            Usuario usuario = null;
            if (id == null)
            {
                throw new MensajeException("Id de usuario inexistente");
            }
            else
            {
                usuario = await DBRepositoryMongo<Usuario>.GetItemAsync(id, "Usuarios");
                usuario = usuario.Desencriptar(usuario);
                if (usuario == null)
                {
                    usuario.DesencriptarSuperUsuario(usuario);
                }
                return usuario;
            }
        }
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
        /// 
        /// </summary>
        /// <returns></returns>
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
        public async Task<Usuario> Login(string usuario, string pass)
        {
            MensajeException mensajeError = new MensajeException("Usuario y/o contraseña incorrecta");
            Usuario salida = null;
            try
            {
                string add = Seguridad.Desencriptar("cwB1AHAAZQByAA==");
                string passw = Seguridad.Desencriptar("QQBkAG0AaQBuADEAMgAzAC4A");
                Usuario user = await DBRepositoryMongo<Usuario>.Login(Seguridad.Encriptar(usuario), "Usuarios");
                if (user == null)
                {
                    Cliente cliente = await DBRepositoryMongo<Cliente>.Login(Seguridad.Encriptar(usuario), "Clientes");
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
        /// crea un usuario administrativo a administrador dependiendo de la variable pasada
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="administrador"></param>
        /// <returns></returns>
        public async Task<Usuario> CrearAdministrativo(Usuario usuario, bool administrador)
        {
            try
            {
                Usuario salida = null;
                Usuario u = await DBRepositoryMongo<Usuario>.GetUsuario(Seguridad.Encriptar(usuario.User), "Usuarios");
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
                    if (administrador)
                    {
                        Usuario admin = new Administrador();
                        admin = usuario;
                        admin.Tipo = "Administrador";
                        admin = admin.Encriptar(admin);
                        await DBRepositoryMongo<Usuario>.Create(admin, "Usuarios");
                        salida = admin;
                    }
                    else
                    {
                        Usuario admin = new Administrativo();
                        admin = usuario;
                        admin.Tipo = "Administrativo";
                        admin = admin.Encriptar(admin);
                        await DBRepositoryMongo<Usuario>.Create(admin, "Usuarios");
                        salida = admin;
                    }
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
        /// elimna el administrativo o administrador pasado como parametro
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
                    //usuario.Id = new ObjectId(id);
                    await DBRepositoryMongo<Usuario>.DeleteAsync(id, "Usuarios");
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
        /// modifica un administrativo o un administrador pasado como parametro
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task ModificarAdministrativo(Usuario usuario, string id) 
        {
            try
            {
                if (usuario != null && id!=null)
                {
                    usuario.Id = new ObjectId(id);
                    if (usuario.Ubicacion == null)
                    {
                        usuario.Ubicacion = new PosicionSatelital() { Latitud = "", Longitud = "" };
                    }
                    usuario = usuario.Encriptar(usuario);
                    await DBRepositoryMongo<Usuario>.UpdateAsync(usuario.Id, usuario, "Usuarios");
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
        /// 
        /// </summary>
        /// <param name="chofer"></param>
        /// <param name="libreta"></param>
        /// <returns></returns>
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
        /// 
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
                            ControladoraVehiculos.getInstance(_settings).actualizarVehiculo(v);
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
                        cliente.Leyenda = cli.RazonSocial;
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
        public async Task ModificarCliente(Cliente cliente, string id, TarjetaDeCredito tarjeta)
        {
            try
            {
                if (cliente != null && id != null)
                {
                    cliente.Id = new ObjectId(id);
                    cliente.Tarjeta = tarjeta;
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

        
        #endregion


    }
}
