SELECT * FROM ( 
	SELECT ESTADO, JMS_ID AS JmsId, DELEGACION FROM SOL_JMS_ENVIO 
	WHERE 
		INTEGRACION = :INTEGRACION 
		AND ATRIBUTO1 = :ATRIBUTO1
		AND ATRIBUTO2 = :ATRIBUTO2
		AND OPERACION = :OPERACION 
		AND DELEGACION = :DELEGACION
		AND ESTADO IN (:ESTADO)
		AND TRUNC(F_ALTA) >= TRUNC(SYSDATE - 5)
	ORDER BY JMS_ID DESC) 
WHERE ROWNUM <= 1 