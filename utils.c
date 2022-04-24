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

struct pair {
    int sockFd;
    char* name;
};

// Return a listening socket
int get_listener_socket(char* host, char* port)
{
    int listener;     // Listening socket descriptor
    int yes=1;        // For setsockopt() SO_REUSEADDR, below
    int rv;

    struct addrinfo hints, *ai, *p;

    // Get us a socket and bind it
    memset(&hints, 0, sizeof hints);
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_flags = AI_PASSIVE;
    if ((rv = getaddrinfo(host, port, &hints, &ai)) != 0) {
        fprintf(stderr, "selectserver: %s\n", gai_strerror(rv));
        exit(1);
    }
    
    for(p = ai; p != NULL; p = p->ai_next) {
        listener = socket(p->ai_family, p->ai_socktype, p->ai_protocol);
        if (listener < 0) { 
            continue;
        }
        
        // Lose the pesky "address already in use" error message
        setsockopt(listener, SOL_SOCKET, SO_REUSEADDR, &yes, sizeof(int));

        if (bind(listener, p->ai_addr, p->ai_addrlen) < 0) {
            close(listener);
            continue;
        }

        break;
    }

    freeaddrinfo(ai); // All done with this

    // If we got here, it means we didn't get bound
    if (p == NULL) {
        return -1;
    }

    // Listen
    if (listen(listener, 10) == -1) {
        return -1;
    }

    return listener;
}

// Valid domains are S1, S2 and S3
// Returns port, if one of these. Else, returns 0
char * getPortByDomain(char* domain)
{
    if (strcmp(domain, "S1") == 0)
        return "20001";
    if (strcmp(domain, "S2") == 0)
        return "20002";
    if (strcmp(domain, "S3") == 0)
        return "20003";
    return "0";
}

int connectToSocket(char* host, char* port) {
    int status;
    struct addrinfo hints;
    struct addrinfo *servinfo;  // will point to the results

    memset(&hints, 0, sizeof hints); // make sure the struct is empty
    hints.ai_family = AF_UNSPEC;     // don't care IPv4 or IPv6
    hints.ai_socktype = SOCK_STREAM; // TCP stream sockets
    hints.ai_flags = AI_PASSIVE;     // fill in my IP for me

    if ((status = getaddrinfo(host, port, &hints, &servinfo)) != 0) {
        printf("getaddrinfo error: %s\n", gai_strerror(status));
        return 1;
    }
    int sockfd = -1;
    int first = 1;
    while(servinfo != NULL) {
        if (!first)
            servinfo = servinfo->ai_next;
        first = 0;
        sockfd = socket(servinfo->ai_family, servinfo->ai_socktype, servinfo->ai_protocol);
        if (sockfd == -1) {
            shutdown(sockfd, 0);
            printf("socket error\n");
            continue;
        }
        if (connect(sockfd, servinfo->ai_addr, servinfo->ai_addrlen) != 0) {
            shutdown(sockfd, 0);
            printf("connect error\n");
            continue;
        }
        break;
    }
    if (sockfd == -1) {
        printf("could not connect socket\n");
        return -1;
    }
    return sockfd;
}