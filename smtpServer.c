#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <poll.h>

#include <unistd.h>
#include <pthread.h>

#include "utils.c"

#define STATUS_CODE_BYE "221 Bye"
#define STATUS_CODE_OK "250 OK"
#define STATUS_CODE_BAD_COMMAND "500 Syntax error, command unrecognized"
#define STATUS_CODE_BAD_PARAMS "501 Syntax error in parameters or arguments"
#define STATUS_CODE_BAD_SEQUENCE "503 Bad sequence of commands"
#define STATUS_CODE_BAD_USER "550 User not available"

// Path to stored files
static char storagePath[128];
static char domain[128];

struct pair servers[2];

void saveEmail(char *sender, char *recipient, char *message, int sockFd)
{
    // Create file name
    int messageNum = 0;

    char filePathPrefix[256];
    sprintf(filePathPrefix, "%s%s/", storagePath, recipient);

    while (1)
    {
        char filename[256];
        sprintf(filename, "%s_%d.txt", sender, messageNum);

        char *filePath = calloc(1024, sizeof(char));
        strcpy(filePath, filePathPrefix);
        strcat(filePath, filename);
        struct stat st = {0};
        if (stat(filePath, &st) == -1)
        {
            FILE *file = fopen(filePath, "w");

            // iterate line by line through message
            char *line = strtok(message, "\r\n");
            while (1)
            {
                if (strcmp(line, ".") == 0)
                    break;
                fprintf(file, "%s\r\n", line);
                line = strtok(NULL, "\r\n");
                if (line == NULL)
                {
                    recv(sockFd, message, 1024, 0);
                    line = strtok(message, "\r\n");
                }
            }
            // fprintf(file, "%s", message);
            fclose(file);
            break;
        }
        messageNum += 1;
    }
}

char *verifyEmail(char *email)
{
    printf("Email: %s", email);
    char expectedEnding[256];
    sprintf(expectedEnding, "@%s.com", domain);

    if (strstr(email, "@") == NULL || strstr(email, ".") == NULL)
    {
        return STATUS_CODE_BAD_PARAMS;
    }
    else if (strstr(email, expectedEnding) == NULL)
    {
        return STATUS_CODE_BAD_USER;
    }

    return STATUS_CODE_OK;
}

char *handleMail(char *mailMessage, int sockFd)
{
    char *sender = calloc(1024, sizeof(char));
    ;
    char *recipient = calloc(1024, sizeof(char));

    char *goodReply = calloc(1024, sizeof(char));
    char *rcptMessage = calloc(1024, sizeof(char));
    char *buffer = calloc(1024, sizeof(char));
    char *dataMessage = calloc(1024, sizeof(char));
    strcpy(goodReply, STATUS_CODE_OK);
    strcat(goodReply, "\r\n");

    if (strstr(mailMessage, "<") == NULL || strstr(mailMessage, ">") == NULL)
    {
        return STATUS_CODE_BAD_PARAMS;
    }

    if (strstr(mailMessage, "mail FROM:") != NULL)
    {
        strcpy(sender, mailMessage);

        sender = strtok(mailMessage, "<");
        sender = strtok(NULL, " ");
        sender[strcspn(sender, ">")] = 0;
        printf("Sender: %s\n", sender);
        send(sockFd, goodReply, strlen(goodReply), 0);
        printf("S%d: %s\n", sockFd, goodReply);
        recv(sockFd, rcptMessage, 1024, 0);
        printf("C%d: %s\n", sockFd, rcptMessage);
    }

    if (strstr(rcptMessage, "<") == NULL || strstr(rcptMessage, ">") == NULL)
    {
        return STATUS_CODE_BAD_PARAMS;
    }

    if (strstr(rcptMessage, "rcpt TO:") != NULL)
    {
        strcpy(recipient, rcptMessage);
        recipient = strtok(rcptMessage, "<");
        recipient = strtok(NULL, " ");
        recipient[strcspn(recipient, ">")] = 0;
        printf("Recipient: %s\n", recipient);

        // Verify recipient
        char *verificationResult = verifyEmail(recipient);
        if (verificationResult != STATUS_CODE_OK)
        {
            return verificationResult;
        }
        // Create directory
        struct stat st = {0};
        char *userDir = calloc(1024, sizeof(char));
        strcpy(userDir, storagePath);
        strcat(userDir, recipient);
        if (stat(userDir, &st) == -1)
        {
            mkdir(userDir, 0700);
        }

        send(sockFd, goodReply, strlen(goodReply), 0);
        printf("S%d: %s\n", sockFd, goodReply);
        recv(sockFd, buffer, 1024, 0);
        printf("C%d: %s\n", sockFd, buffer);
    }
    else if (strstr(rcptMessage, "rset") != NULL)
        return STATUS_CODE_OK;
    else
        return STATUS_CODE_BAD_SEQUENCE;

    if (strstr(buffer, "data"))
    {
        send(sockFd, "354\r\n", 6, 0);
        printf("S%d: %s\n", sockFd, "354\r\n");
        recv(sockFd, dataMessage, 1024, 0);
        printf("C%d: %s\n", sockFd, dataMessage);

        saveEmail(sender, recipient, dataMessage, sockFd);
    }
    else if (strstr(buffer, "rset") != NULL)
        return STATUS_CODE_OK;
    else
        return STATUS_CODE_BAD_SEQUENCE;

    // \x00 sometimes randomly appearing at the start of the message sent to the client

    return STATUS_CODE_OK;
}

// Returns a response string based on gotten message.
char *parseMessage(char *message, int sockFd)
{
    char *answer = calloc(2048, sizeof(char));

    if (strstr(message, "noop"))
    {
        strcpy(answer, STATUS_CODE_OK);
    }
    else if (strstr(message, "helo"))
    {
        strcpy(answer, STATUS_CODE_OK);
    }
    else if (strstr(message, "ehlo"))
    {
        strcpy(answer, STATUS_CODE_OK);
        strcat(answer, ". Supported commands: HELO, EHLO, MAIL, RCPT, DATA, NOOP, RSET, VRFY, QUIT");
    }
    else if (strstr(message, "vrfy"))
    {
        message = message + 5;
        strcpy(answer, verifyEmail(message));
    }
    else if (strstr(message, "QUIT"))
    {
        strcpy(answer, STATUS_CODE_BYE);
    }
    else if (strstr(message, "mail"))
    {
        strcpy(answer, handleMail(message, sockFd));
    }
    else if (strstr(message, "rset"))
    {
        strcpy(answer, STATUS_CODE_OK);
    }
    else
    {
        strcpy(answer, STATUS_CODE_BAD_COMMAND);
    }

    // end with <crlf>
    strcat(answer, "\r\n");

    return answer;
}

void *satisfyClient(void *arg)
{
    int sockFd = *(int *)arg;
    while (1)
    {
        char *buffer = (char *)calloc(1024, sizeof(char));

        int numbytes = recv(sockFd, buffer, 1024, 0);
        printf("C%d: %s\n", sockFd, buffer);

        if (numbytes == -1)
        {
            printf("recv error on socket %d\n", sockFd);
            break;
        }
        if (numbytes == 0)
        {
            printf("connection closed on socket %d\n", sockFd);
            break;
        }

        char *response = parseMessage(buffer, sockFd);
        printf("S%d: %s\n", sockFd, response);

        send(sockFd, response, strlen(response), 0);

        if (strstr(response, STATUS_CODE_BYE))
        {
            break;
        }
        // free(response);
        free(buffer);
    }
    close(sockFd);
}

void connectToServers(char *host)
{
    if (strcmp(domain, "S1"))
    {
        servers[0].name = "S2";
        servers[0].sockFd = connectToSocket(host, "20002");
        servers[1].name = "S3";
        servers[1].sockFd = connectToSocket(host, "20003");
    }
    else if (strcmp(domain, "S2"))
    {
        servers[0].name = "S1";
        servers[0].sockFd = connectToSocket(host, "20001");
        servers[1].name = "S3";
        servers[1].sockFd = connectToSocket(host, "20003");
    }
    else if (strcmp(domain, "S3"))
    {
        servers[0].name = "S1";
        servers[0].sockFd = connectToSocket(host, "20001");
        servers[1].name = "S2";
        servers[1].sockFd = connectToSocket(host, "20002");
    }
}

// First argument is server name
int main(int argc, char const *argv[])
{
    char *host = "::1";
    char *port = calloc(128, sizeof(char));

    strcpy(domain, argv[1]);
    port = getPortByDomain(domain);
    if (strcmp(port, "0") == 0)
        ;
    {
        fprintf(stderr, "Invalid domain\n");
        exit(1);
    }
    sprintf(storagePath, "./%s/", domain);

    int listeningSocket = get_listener_socket(host, port);
    if (listeningSocket == -1)
    {
        fprintf(stderr, "Failed to get listening socket\n");
        exit(1);
    }

    // User file uploads
    struct stat st = {0};
    if (stat(storagePath, &st) == -1)
    {
        mkdir(storagePath, 0700);
    }

    printf("Input any character when all servers have been turned on.\n");
    getchar();

    // connectToServers(host);

    printf("Waiting for connections...\n");
    while (1)
    {
        struct sockaddr_storage their_addr;
        socklen_t addr_size = sizeof their_addr;

        int newFd = accept(listeningSocket, (struct sockaddr *)&their_addr, &addr_size);
        if (newFd == -1)
            printf("accept error\n");
        printf("Accepted connection on socket %d\n", newFd);
        send(newFd, "220 Service ready\r\n", 20, 0);

        pthread_t tid;
        pthread_create(&tid, NULL, satisfyClient, (void *)&newFd);
    }

    shutdown(listeningSocket, 0);
    pthread_exit(NULL);
}