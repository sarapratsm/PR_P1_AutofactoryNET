using Microsoft.EntityFrameworkCore;
using System;

namespace DAO
{
    public class UnitOfWork : IDisposable
    {
        private readonly MyDBContext _context;
        private bool _disposed = false;

        // Les teves propietats "Lazy Loading" (ESTÀ PERFECTE)
        private IDAOComponente _daoComponente;
        private IDAOProducte _daoProducte;
        private IDAOProveidor _daoProveidor;

        public UnitOfWork(MyDBContext context)
        {
            _context = context;
        }

        public IDAOComponente DAOComponente
        {
            get
            {
                if (_daoComponente == null)
                {
                    _daoComponente = new DAOComponente(_context);
                }
                return _daoComponente;
            }
        }

        public IDAOProducte DAOProducte
        {
            get
            {
                if (_daoProducte == null)
                {
                    _daoProducte = new DAOProducte(_context);
                }
                return _daoProducte;
            }
        }
        public IDAOProveidor DAOProveidor
        {
            get
            {
                if (_daoProveidor == null)
                {
                    _daoProveidor = new DAOProveidor(_context);
                }
                return _daoProveidor;
            }
        }

        // --- MÈTODES DEL TEU PROFESSOR (APROFITATS) ---

        /// <summary>
        /// Inicia una transacció manual a la base de dades.
        /// </summary>
        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        /// <summary>
        /// Confirma tots els canvis (tant els de EF com els de SQL manual)
        /// que s'han fet dins d'aquesta transacció.
        /// </summary>
        public void Commit()
        {
            try
            {
                // 1. Primer, desa els canvis pendents d'Entity Framework (si n'hi hagués)
                _context.SaveChanges();

                // 2. Després, confirma la transacció manual a la BBDD
                _context.Database.CurrentTransaction?.Commit();
            }
            catch
            {
                // Si falla, fem rollback
                Rollback();
                throw; // Rellença l'excepció
            }
        }

        /// <summary>
        /// Desfà tots els canvis fets dins d'aquesta transacció.
        /// </summary>
        public void Rollback()
        {
            _context.Database.CurrentTransaction?.Rollback();
        }

        /// <summary>
        /// Allibera el context i la connexió.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // Si hi ha una transacció pendent que no s'ha confirmat, es fa rollback
                _context.Database.CurrentTransaction?.Rollback();
                _context.Dispose(); // Tanca la connexió
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}