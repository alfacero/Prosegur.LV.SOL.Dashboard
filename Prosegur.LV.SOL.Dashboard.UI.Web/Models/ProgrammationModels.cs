using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Prosegur.LV.SOL.Dashboard.UI.Web.Models
{

    public class ProgrammationStateSummaryViewModel
    {
        public int Quantity { set; get; }

        public ProgrammationStateModel ProgrammationState { set; get; }
    }

    public class ProgrammationStateModel
    {
        public int Code { set; get; }

        public string Description { set; get; }

        private static List<ProgrammationStateModel> _programmationStates;

        public static List<ProgrammationStateModel> GetAll()
        {
            if (_programmationStates == null)
            {
                _programmationStates = new List<ProgrammationStateModel>();

                _programmationStates.Add(new ProgrammationStateModel
                {
                    Code = 0,
                    Description = "Pendiente SOL",
                });

                _programmationStates.Add(new ProgrammationStateModel
                {
                    Code = 1,
                    Description = "Cerrado SOL - Pendiente Integración",
                });

                _programmationStates.Add(new ProgrammationStateModel
                {
                    Code = 2,
                    Description = "Cerrado SOL - Error Integración",
                });

                _programmationStates.Add(new ProgrammationStateModel
                {
                    Code = 3,
                    Description = "Cerrado SOL - Integracion Correcta - SIGII Integrado",
                });

                _programmationStates.Add(new ProgrammationStateModel
                {
                    Code = 4,
                    Description = "Cerrado SOL - Integracion Correcta - SIGII NO Integrado",
                });
            }

            return _programmationStates;
        }

        public static ProgrammationStateModel GetByCode(int code)
        {
            ProgrammationStateModel.GetAll();

            return _programmationStates.Where(s => s.Code.Equals(code)).FirstOrDefault();
        }

        public static ProgrammationStateModel ConvertFrom(Entities.SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado solIntegracionProgramacionEstado)
        {
            ProgrammationStateModel stateModel = GetByCode(0);

            switch (solIntegracionProgramacionEstado)
            { 
                case Entities.SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.PendienteSOL:
                    stateModel = GetByCode(0);
                    break;
                case Entities.SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLPendienteIntegracion:
                    stateModel = GetByCode(1);
                    break;
                case Entities.SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLErrorIntegracion:
                    stateModel = GetByCode(2);
                    break;
                case Entities.SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIIIntegrado:
                    stateModel = GetByCode(3);
                    break;
                case Entities.SOLIntegracionProgramacion.SOLIntegracionProgramacionEstado.CerradoSOLIntegracionCorrectaSIGIINoIntegrado:
                    stateModel = GetByCode(4);
                    break;
            }
    
            return stateModel;
        }

    }
    public class ProgrammationFilterViewModel
    {
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Debe ingresar una fecha de programación")]
        public DateTime ProgrammationDate { set; get; }

        [Required(ErrorMessage = "Debe seleccionar al menos una delegación")]
        public List<string> DelegationCodes { set; get; }

        [Display(Name = "Delegación")]
        public IEnumerable<SelectListItem> Delegations { set; get; }

        public IEnumerable<ProgrammationStateSummaryViewModel> ProgrammationStatesSummary { set; get; }

        public IEnumerable<string> SigIIProgrammationPendings { set; get; }

        public IEnumerable<int> ErrorMessagesId { set; get; }
    }

    public class ProgrammationBarFilterViewModel
    {
        public ProgrammationBarFilterViewModel()
        {
            IEnumerable<int> days = new List<int> { -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5 };

            var itemDays = from d in days
                           select new { Code = d, Description = d };

            ProgrammationServiceOffset = 1;

            _days = new SelectList(itemDays, "Code", "Description");
        }

        [Display(Name = "Dias")]
        [Required(ErrorMessage = "Debe seleccionar el día")]
        public int ProgrammationServiceOffset { set; get; }

        private IEnumerable<SelectListItem> _days;

        public IEnumerable<SelectListItem> Days {
            get
            {
                return _days;
            }
        }

        [Required(ErrorMessage = "Debe seleccionar al menos una delegación")]
        public List<string> DelegationCodes { set; get; }

        [Display(Name = "Delegación")]
        public IEnumerable<SelectListItem> Delegations { set; get; }

        public ProgrammationBarStateViewModel ProgrammationStates { set; get; }

        public IEnumerable<string> SigIIProgrammationPendings { set; get; }

        public IEnumerable<string> ErrorMessagesId { set; get; }
    }

     //CierresFactPorDelegacionPartial
     public class CierreFactFilterViewModel
    {

       
        public CierreFactFilterViewModel()
        {
            IEnumerable<int> days = new List<int> { -9,-8,-7,-6,-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5,6,7,8,9 };

            var itemDays = from d in days
                           select new { Code = d, Description = d };

            ProgrammationServiceOffset = 1;

            _days = new SelectList(itemDays, "Code", "Description");
        }

        [Display(Name = "Dias")]
        [Required(ErrorMessage = "Debe seleccionar el día")]
        public int ProgrammationServiceOffset { set; get; }

        private IEnumerable<SelectListItem> _days;

        public IEnumerable<SelectListItem> Days
        {
            get
            {
                return _days;
            }
        }

        [Required(ErrorMessage = "Debe seleccionar al menos una delegación")]
        public List<string> DelegationCodes { set; get; }

        [Display(Name = "Delegación")]
        public IEnumerable<SelectListItem> Delegations { set; get; }

        public ProgrammationBarStateViewModel ProgrammationStates { set; get; }

        public IEnumerable<string> SigIIProgrammationPendings { set; get; }

        public IEnumerable<string> ErrorMessagesId { set; get; }

    }



    public class ProgrammationBarStateViewModel
    {
        public IEnumerable<DelegationModel> Delegations;

        public IEnumerable<IntegrationBarStateSummaryModel> ProgrammationStates { set; get; }
    }

    public class IntegrationBarStateSummaryModel
    {
        public IEnumerable<int[]> DelegationsQuantity { set; get; }

        public IntegrationBarStateModel State { set; get; }
    }

    public class IntegrationBarStateModel
    {
        public int Code { set; get; }       

        public string Description { set; get; }

        public string HexaColor { set; get; }

        private static List<IntegrationBarStateModel> _programmationStates;

        public static List<IntegrationBarStateModel> GetAll()
        {
            if (_programmationStates == null)
            {
                _programmationStates = new List<IntegrationBarStateModel>();

                _programmationStates.Add(new IntegrationBarStateModel
                {
                    Code = 0,
                    Description = "A Cerrar",
                    HexaColor = "#0000FF"
                });

                _programmationStates.Add(new IntegrationBarStateModel
                {
                    Code = 1,
                    Description = "Integrando",
                    HexaColor = "#ffff00"
                });

                _programmationStates.Add(new IntegrationBarStateModel
                {
                    Code = 2,
                    Description = "Integrado",
                    HexaColor = "#008B00"
                });

                _programmationStates.Add(new IntegrationBarStateModel
                {
                    Code = 3,
                    Description = "Error",
                    HexaColor = "#ff0000"
                });

            }

            return _programmationStates;
        }

        public static IntegrationBarStateModel GetByCode(int code)
        {
            IntegrationBarStateModel.GetAll();

            return _programmationStates.Where(s => s.Code.Equals(code)).FirstOrDefault();
        }

        public static IntegrationBarStateModel ConvertFrom(Entities.SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta solIntegracionProgramacionEstado)
        {
            IntegrationBarStateModel stateModel = GetByCode(0);

            switch (solIntegracionProgramacionEstado)
            {
                case Entities.SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.ACerrar:
                    stateModel = GetByCode(0);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Integrando:
                    stateModel = GetByCode(1);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Integrado:
                    stateModel = GetByCode(2);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacionRuta.EstadosIntegracionProgramacionDelegacionRuta.Error:
                    stateModel = GetByCode(3);
                    break;
            }

            return stateModel;
        }

        public static IntegrationBarStateModel ConvertFrom(Entities.SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion solIntegracionProgramacionEstado)
        {
            IntegrationBarStateModel stateModel = GetByCode(0);

            switch (solIntegracionProgramacionEstado)
            {
                case Entities.SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.ACerrar:
                    stateModel = GetByCode(0);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Integrando:
                    stateModel = GetByCode(1);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Integrada:
                    stateModel = GetByCode(2);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacion.EstadosIntegracionProgramacionDelegacion.Error:
                    stateModel = GetByCode(3);
                    break;
            }

            return stateModel;
        }

        public static IntegrationBarStateModel ConvertFrom(Entities.SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo solIntegracionProgramacionEstado)
        {
            IntegrationBarStateModel stateModel = GetByCode(0);

            switch (solIntegracionProgramacionEstado)
            {
                case Entities.SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.ACerrar:
                    stateModel = GetByCode(0);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.Integrando:
                    stateModel = GetByCode(1);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.Integrado:
                    stateModel = GetByCode(2);
                    break;
                case Entities.SOLIntegracionProgramacionDelegacionSector.EstadosIntegracionProgramacionDelegacionGrupo.Error:
                    stateModel = GetByCode(3);
                    break;
            }

            return stateModel;
        }
    }
}