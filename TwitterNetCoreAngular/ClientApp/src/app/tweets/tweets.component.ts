import { Component, Inject, OnInit } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { TweetsService } from './tweets.service';

@Component({
  selector: 'app-tweets',
  templateUrl: './tweets.component.html',
  styleUrls: ['./tweets.component.css']
})
export class TweetsComponent implements OnInit {
  baseUrl = '';
  data: any = [];

  connection = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Information)
    .withUrl(this.baseUrl + 'dataHub')
    .withAutomaticReconnect()
    .build();

  constructor(private tweetsService: TweetsService, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  ngOnInit(): void {
    this.signal();
  }

  signal() {
    this.connection.serverTimeoutInMilliseconds = 9000000000
    this.connection.start().then(() => {
      console.log('user connected!');
      this.get();
    }).catch((err) => console.log(err));

    this.connection.on('BroadcastMessage', () => {
      console.log('notification received!');
      this.get();
    });
    this.connection.onreconnected(() => {
      console.log('user reconnected!');
      this.get();
    });
  }

  get() {
    this.tweetsService.getAll().then((data) => {
      console.log(data);
      this.data = data;
    }).catch((err) => console.log(err));
  }

}
