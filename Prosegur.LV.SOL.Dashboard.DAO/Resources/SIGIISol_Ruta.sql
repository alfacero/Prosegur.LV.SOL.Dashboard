select b.DELEGACION as Delegacion,
	   a.RECORRIDO as Recorrido,
	   a.REVISION as Revision from sol_ruta a
	   join sol_planta b on 
    a.empresa = b.empresa and
	a.planta =b.planta
where a.empresa = 1 
	and f_servicio = @F_SERVICIO
	and b.delegacion IN (@DELEGACION)