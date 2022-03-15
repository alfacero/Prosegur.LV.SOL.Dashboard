SELECT DISTINCT J1.ID, J1.OBSERVACION, J1.DELEGACION FROM SOL_JMS_ENVIO J1 
WHERE J1.F_ALTA > SYSDATE - :desdeHoras/24 AND J1.DELEGACION IN (:codigoDelegacion) 
      AND J1.INTEGRACION = :integracion AND J1.ESTADO = :estadoError
      AND NOT EXISTS (SELECT 1 FROM SOL_JMS_ENVIO J2 
                      WHERE J2.F_ALTA > J1.F_ALTA 
                            AND J2.ID = J1.ID 
                            AND J2.INTEGRACION = J1.INTEGRACION)