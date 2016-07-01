FROM mono:4.2

RUN mkdir /app
COPY . /app

EXPOSE 8083

ENV MONO_THREADS_PER_CPU=2000

ENTRYPOINT ["mono", "/app/BuildStats.Suave.exe", "-d"]
CMD ["--server"]