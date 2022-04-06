# Proyecto-de-Redes

Para ejecutar el proyecto se debe tener instalado
 [dotnet>=6.0.101](https://dotnet.microsoft.com/en-us/download) abrir la carpeta desde una consola
y ejecutar el siguiente comando:

```dotnet
dotnet run --project Test/Test.csproj
```

En el fichero ```config.txt``` se pueden ajustar varios parámetros para correr el programa

1. **signal_time** : este es el tiempo que un bit tiene que estar transmitiéndose para que sea recibido

2. **numero_puertos_hub**: Este parámetro son 2 enteros de la forma ***a-b*** separados por el caracter **'-'**  el primer entero ***a*** corresponde con la cantidad mínima de puertos que puede tener un *hub* . Y el segundo ***b*** es la cantidad maxima de puertos que puede tener un hub .

3. **max_cantidad_milisegundos**: Este parámetro es la cantidad máxima de mili-segundos que correrá el programa , después de este tiempo el programa finaliza sin ejecutar ninguna otra instrucción.
