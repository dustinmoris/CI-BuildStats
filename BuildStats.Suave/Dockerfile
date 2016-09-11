FROM ubuntu:14.04

RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF

RUN echo "deb http://download.mono-project.com/repo/debian wheezy main" | tee /etc/apt/sources.list.d/mono-xamarin.list
RUN echo "deb http://download.mono-project.com/repo/debian wheezy-apache24-compat main" | tee -a /etc/apt/sources.list.d/mono-xamarin.list

RUN apt-get update -y

RUN apt-get install mono-devel -y
RUN apt-get install ca-certificates-mono -y
RUN apt-get install referenceassemblies-pcl -y
RUN apt-get install mono-xsp4 -y
RUN apt-get install fsharp -y

ENV MONO_THREADS_PER_CPU 500

RUN mkdir /app
COPY . /app

ENTRYPOINT ["mono", "/app/BuildStats.Suave.exe"]