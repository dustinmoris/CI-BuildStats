FROM mono:4

RUN mkdir /app
COPY . /app

ENV MONO_THREADS_PER_CPU=5000

ENTRYPOINT ["mono", "/app/BuildStats.Suave.exe", "--server"]