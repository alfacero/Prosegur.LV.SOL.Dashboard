SELECT 
	JMS_ID as JmsId,
	ID,
--	MENSAJE,
--	OBSERVACION,
	ESTADO, 
--	ATRIBUTO1, 
--	ATRIBUTO2, 
--	INTEGRACION,
	DELEGACION
FROM SOL_JMS_ENVIO s
   WHERE    TRUNC (f_alta) >= TRUNC(:FEC_ALTA) --AND TRUNC(f_alta) < trunc(:FEC_ALTA) + 2
         AND operacion = 'M'
         AND integracion ='RUTA'
         and delegacion in(:DELEGACION)
         AND ESTADO IN ('PE','ER','OK')
         AND atributo2 IN ('6','7') -- estado FINALIZADA, estado CERRADA (graba los documentos de SOL en SigII) 
--ORDER BY delegacion,
--        f_ult_mod DESC,
--        f_alta DESC