  SELECT RUT.OID_RUTA AS OidRuta,
         DELE.COD_DELEGACION AS CodigoDelegacion,
         RUT.COD_RUTA AS CodigoRuta,
         RUT.COD_ESTADO AS CodigoEstado
    FROM SLDL_TRUTA RUT
         JOIN SLDG_TDELEGACION DELE ON RUT.OID_DELEGACION = DELE.OID_DELEGACION
   WHERE     RUT.COD_ESTADO IN (6, 7)
         AND RUT.FEC_RUTA = TRUNC ( :FEC_RUTA)
         AND DELE.COD_DELEGACION IN ( :DELEGACIONES)
ORDER BY 1, 2, 3