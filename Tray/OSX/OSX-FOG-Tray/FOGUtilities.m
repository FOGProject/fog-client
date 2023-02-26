/*
 * FOG Service : A computer management client for the FOG Project
 * Copyright (C) 2014-2023 FOG Project
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */


#import "FOGUtilities.h"
#import <SocketRocket/SRWebSocket.h>
@interface FOGUtilities ()
@property (strong, nonatomic) SRWebSocket *webSocket;
@property (strong, nonatomic) NSStatusItem *statusItem;
@property (strong, nonatomic) NSMenu *menu;
@end

@implementation FOGUtilities




-(void) setup {
    ProcessSerialNumber psn = { 0, kCurrentProcess };
    TransformProcessType(&psn, kProcessTransformToUIElementApplication);
    [[NSUserNotificationCenter defaultUserNotificationCenter] setDelegate:self];
    [self startWSClient];
    [self createSysTray];
    NSString *myCMD = [self runCMD:@"cat /opt/fog-service/settings.json"];
    NSDictionary *myStettings = [self toJSON:myCMD];
    NSString *myVersion = [myStettings objectForKey:@"Version"];
    [self setVersion:myVersion];
}

-(void) tearDown {
    [self.webSocket close];
}


// System Tray stuff
-(void)addMenuItem:(NSMenuItem *)newItem {
    [self.menu addItem: newItem];
}

-(void) createSysTray{
    
    _statusItem = [[NSStatusBar systemStatusBar] statusItemWithLength:NSVariableStatusItemLength];
    self.statusItem.button.image = [NSImage imageNamed:@"9626396"];
    self.statusItem.alternateImage = [NSImage imageNamed:@"9626396"];
    [self.statusItem setToolTip:@"FOG"];
    self.menu = [[NSMenu alloc] init];
    [self.menu setTitle:@"Fog Tray"];
    [self.menu addItem:[NSMenuItem separatorItem]];
    self.statusItem.menu = self.menu;
    
}

-(void) setVersion: (NSString*) myVersion {
    [self.statusItem setToolTip:[NSString stringWithFormat:@"FOG v%@",myVersion]];
}





// WebSocket Stuff
-(void) startWSClient {
    self.webSocket.delegate = nil;
    self.webSocket = nil;
    NSString *urlString = @"ws://localhost:1277";
    SRWebSocket *newWebSocket = [[SRWebSocket alloc] initWithURL:[NSURL URLWithString:urlString]];
    newWebSocket.delegate = self;
    [newWebSocket open];
}


- (void)webSocketDidOpen:(SRWebSocket *)newWebSocket {
    self.webSocket = newWebSocket;
    NSLog(@"WebSocket Connected");
}

- (void)webSocket:(SRWebSocket *)webSocket didFailWithError:(NSError *)error {
    NSLog(@"Failed Connecting to Bus, trying again in 30 sec");
    [self performSelector:@selector(startWSClient) withObject:self afterDelay:30.0 ];
}

- (void)webSocket:(SRWebSocket *)webSocket didCloseWithCode:(NSInteger)code reason:(NSString *)reason wasClean:(BOOL)wasClean {
    NSLog(@"Disconnected from Bus, trying again in 30 sec");
    [self performSelector:@selector(startWSClient) withObject:self afterDelay:30.0 ];
}

- (void)webSocket:(SRWebSocket *)webSocket didReceiveMessage:(id)message {
    NSDictionary *jsonData = [self toJSON:message];
    NSString *channel = [jsonData objectForKey:@"channel"];
    NSLog(@"%@",channel);
    NSDictionary *data = [self toJSON:jsonData[@"data"]];
    
    if ([channel isEqualToString:@"Notification"]){
        NSString *title = [data objectForKey:@"title"];
        NSString *message = [data objectForKey:@"message"];
        [self notifyUser:title second:message];
    }else if ([channel isEqualToString:@"Update"]){
        NSString *action = [data objectForKey:@"action"];
        if ([action isEqualToString:@"start"]){
            [[NSApplication sharedApplication] terminate:nil];
        }
    }else if ([channel isEqualToString:@"Status"]){
        NSString *action = [data objectForKey:@"action"];
        if ([action isEqualToString:@"unload"]){
            [[NSApplication sharedApplication] terminate:nil];
        }
    }
}

-(void)sendWSMessage: (NSString*) outMSG {
    [self.webSocket send: outMSG];
}




//JSON Stuff

-(NSDictionary*) toJSON: (NSString*)input {
    NSError *jsonError;
    NSData *objectData = [input dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary *json = [NSJSONSerialization JSONObjectWithData:objectData
                                                         options:kNilOptions
                                                           error:&jsonError];
    return json;
}


// Run CMD

-(NSString*)runCMD:(NSString*)cmd {
    NSPipe *pipe = [NSPipe pipe];
    NSFileHandle *file = pipe.fileHandleForReading;
    
    NSTask *task = [[NSTask alloc] init];
    task.launchPath = @"/bin/bash";
    task.arguments = @[@"-c", cmd];
    task.standardOutput = pipe;
    
    [task launch];
    
    NSData *data = [file readDataToEndOfFile];
    [file closeFile];
    
    NSString *cmdOutput = [[NSString alloc] initWithData: data encoding: NSUTF8StringEncoding];
    return cmdOutput;
}



// User Notifications

-(void) notifyUser: (NSString*) myTitle second:(NSString*)msg
{
    NSUserNotification *notification = [[NSUserNotification alloc] init];
    notification.title = @"FOG Notification";
    notification.subtitle = [NSString stringWithFormat:@"%@",myTitle];
    notification.hasActionButton =  TRUE;
    notification.otherButtonTitle =@"Close";
    notification.informativeText = [NSString stringWithFormat:@"%@",msg];
    notification.soundName = NSUserNotificationDefaultSoundName;
    [[NSUserNotificationCenter defaultUserNotificationCenter] deliverNotification:notification];
}

- (BOOL)userNotificationCenter:(NSUserNotificationCenter *)center shouldPresentNotification:(NSUserNotification *)notification{
    return YES;
}

@end