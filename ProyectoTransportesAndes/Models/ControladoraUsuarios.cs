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
        }
        #endregion

        #region Metodos
        //Devuelve una lista de usuarios de tipo pasado por parametro
        public async Task<IEnumerable<Usuario>> getAdministrativos()
        {
            try
            {
                var items = await DBRepositoryMongo<Usuario>.GetItemsAsync("Usuarios");
                return items;
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
                return usuario;
            }
        }
        public async Task<IEnumerable<Chofer>> getChoferes()
        {
            try
            {
                var items = await DBRepositoryMongo<Chofer>.GetItemsAsync("Choferes");
                return items;
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
                return cliente;
            }
        }
        public async Task<IEnumerable<Cliente>> getClientes()
        {
            try
            {
                var items = await DBRepositoryMongo<Cliente>.GetItemsAsync("Clientes");
                return items;
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
                Usuario user = await DBRepositoryMongo<Usuario>.Login(usuario, "Usuarios");
                if (user == null)
                {
                    Cliente cliente = await DBRepositoryMongo<Cliente>.Login(usuario, "Clientes");
                    if (cliente != null)
                    {
                        if (cliente.Password == pass)
                        {
                            salida = cliente; ;
                        }
                        else
                        {
                            throw mensajeError;
                        }
                    }
                    else
                    {
                        throw mensajeError;
                    }
                }
                else
                {
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
                Chofer chofer = await DBRepositoryMongo<Chofer>.Login(usuario, "Choferes");
                if (chofer != null)
                {
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
        public async Task<Usuario> CrearAdministrativo(Usuario usuario, bool administrador)
        {
            try
            {
                Usuario salida = null;
                Usuario u = await DBRepositoryMongo<Usuario>.GetUsuario(usuario.User, "Usuarios");
                if (u != null)
                {
                    throw new MensajeException("El usuario ya existe");
                }
                else
                {
                    if (administrador)
                    {
                        Usuario admin = new Administrador();
                        admin = usuario;
                        admin.Tipo = "Administrador";
                        await DBRepositoryMongo<Usuario>.Create(admin, "Usuarios");
                        salida = admin;
                    }
                    else
                    {
                        Usuario admin = new Administrativo();
                        admin = usuario;
                        admin.Tipo = "Administrativo";
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
        public async Task EliminarAdministrativo(Usuario usuario, string id)
        {
            try
            {
                if(usuario !=null && id != null)
                {
                    usuario.Id = new ObjectId(id);
                    await DBRepositoryMongo<Usuario>.DeleteAsync(usuario.Id, "Usuarios");
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
        public async Task ModificarAdministrativo(Usuario usuario, string id) 
        {
            try
            {
                if (usuario != null && id!=null)
                {
                    usuario.Id = new ObjectId(id);
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
        public async Task ModificarChofer(Chofer chofer, string id)
        {
            try
            {
                if (chofer != null && id != null)
                {
                    chofer.Id = new ObjectId(id);
                    await DBRepositoryMongo<Chofer>.UpdateAsync(chofer.Id, chofer, "Choferes");
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
        public async Task EliminarChofer(Chofer chofer, string id)
        {
            try
            {
                if (chofer != null && id != null)
                {
                    chofer.Id = new ObjectId(id);
                    await DBRepositoryMongo<Usuario>.DeleteAsync(chofer.Id, "Choferes");
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
        public async Task CrearCliente(Cliente cliente, TarjetaDeCredito tarjeta)
        {
            try
            {
                Cliente salida = null;
                Usuario usuario = await DBRepositoryMongo<Usuario>.GetUsuario(cliente.User, "Usuarios");
                Cliente cli = await DBRepositoryMongo<Cliente>.GetUsuario(cliente.User, "Clientes");
                Chofer chofer = await DBRepositoryMongo<Chofer>.GetUsuario(cliente.User, "Choferes");
                if (usuario == null && cli == null && chofer == null)
                {
                    salida = cliente;
                    salida.Tipo = "Cliente";
                    salida.Tarjeta = tarjeta;
                    if (salida.RazonSocial != null)
                    {
                        salida.Leyenda = cli.RazonSocial;
                    }
                    salida.Leyenda = cliente.Nombre + " " + cliente.Apellido;
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
                    cliente.Id = new ObjectId(id);
                    await DBRepositoryMongo<Usuario>.DeleteAsync(cliente.Id, "Clientes");
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
