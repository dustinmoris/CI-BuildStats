FROM mono:4.2

RUN mkdir /app
COPY . /app

EXPOSE 8083

CMD ["mono", "/app/BuildStats.Suave.exe", "-d"]