SELECT Jms_Id AS JmsId FROM SOL_JMS_ENVIO 
WHERE F_ALTA > SYSDATE - :desdeHoras 
    AND DELEGACION IN (:codigoDelegacion) 
    AND INTEGRACION = :integracion
    AND ESTADO = :estado