﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealCards : MonoBehaviour {

    GameObject[] players;
    private GameObject discardImage;
    private GameObject discardPile;

    // used to keep track of the current dealer.
    public int dealerNumber;
    // used to keep track of whos turn it is.
    public int currentPlayerIndex;

    private GameObject deck;

	// Use this for initialization
	void Start () {

        dealerNumber = 0;
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");
        GameObject player3 = GameObject.Find("Player3");
        GameObject player4 = GameObject.Find("Player4");
        discardPile = GameObject.Find("DiscardPile");
        deck = GameObject.Find("Deck");
        players = new GameObject[]{player1, player2, player3, player4};
        currentPlayerIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {


	}

    // Manager method used to set the over all flow of the match
    public void deal()
    {
        // reset the current deck
        deck.GetComponent<DeckScript>().shuffle();
        //for each player deal 5 cards
        // TODO use the dealer index to start dealing to the next player.
        // TODO re write deal to deal one card at a time not 5, 5, 5, 5
        foreach (GameObject player in players)
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject card = deck.GetComponent<DeckScript>().draw();
                card.GetComponent<CardScript>().setOwner(player.GetComponent<PlayerScript>().playerNumber);
                player.GetComponent<PlayerScript>().addCard(card, i);
            }
            setPokerHand(player);
        }
    }

    // this method gets called when the player clicks the discard button.
    public void playerDiscard()
    {
        for (int i = 0; i < players.Length; i++)
        {
            GameObject currentPlayer = players[currentPlayerIndex];
            GameObject[] cards = currentPlayer.GetComponent<PlayerScript>().unityCard;

            // let the AI take their turn
            if (currentPlayer.GetComponent<PlayerScript>().playerNumber != 1)
            {
                var aiDecider = new AiDecider();
                var discardCards = aiDecider.aiTurn(currentPlayer.GetComponent<PlayerScript>().hand);

                foreach (Card removing in discardCards)
                {
                    var removingIndex = currentPlayer.GetComponent<PlayerScript>().getCardIndex(removing);
                    currentPlayer.GetComponent<PlayerScript>().addCard(deck.GetComponent<DeckScript>().draw(), removingIndex);
                }
            }

            //Player selection
            else if (currentPlayer.GetComponent<PlayerScript>().playerNumber == 1)
            {
                foreach (GameObject card in cards)
                {
                    if (card != null)
                    {
                        //GameObject cardGameObject = GameObject.Find(card.getUnityMapping() + "(Clone)");
                        if (card.GetComponent<CardScript>().selected)
                        {
                            var discardIndex = currentPlayer.GetComponent<PlayerScript>().getCardIndex(card);
                            currentPlayer.GetComponent<PlayerScript>().replaceCard(deck.GetComponent<DeckScript>().draw(), discardIndex, card);
                            
                            discardPile.GetComponent<DiscardPileScript>().add(card);
                        }
                    }
                }
            }

            currentPlayerIndex = (currentPlayerIndex % 3) + 1;
        }
        getWinningHand();

        resetTable();
        //deal();
    }

    private void resetTable()
    {
        foreach (GameObject player in players)
        {
            for (int i = 0; i < 5; i++)
            {
                discardPile.GetComponent<DiscardPileScript>().add(player.GetComponent<PlayerScript>().unityCard[i]);
            }
            player.GetComponent<PlayerScript>().resetHand();          
        }
        discardPile.GetComponent<DiscardPileScript>().reset();
        GameObject[] remainingCards = GameObject.FindGameObjectsWithTag("discardPile");

        foreach (GameObject card in remainingCards)
        {
            Destroy(card);
        }
    }

    private void getWinningHand()
    {

        foreach (GameObject player in players)
        {
            setPokerHand(player);
        }

        foreach (GameObject player in players)
        {
            setPokerHand(player);
        }
        isWinner();
        dealerNumber = (dealerNumber % 3) + 1;
    }

    private void setPokerHand(GameObject player)
    {
        PokerHand pokerHand;

        var playerScript = player.GetComponent<PlayerScript>();
        playerScript.sortHand();

        pokerHand = new PokerHand();
        pokerHand.setPokerHand(playerScript.getCards());
        playerScript.hand = pokerHand;
        
        Debug.Log( "Player num: " +playerScript.playerNumber + " " + pokerHand.printResult());     
    }

    private void isWinner()
    {
        int highHand = 0;
        List<GameObject> winner = new List<GameObject>();

        // Identify the highest hand
        foreach (GameObject p in players)
        {
            if (p.GetComponent<PlayerScript>().hand.strength > highHand)
                highHand = p.GetComponent<PlayerScript>().hand.strength;
        }

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PlayerScript>().hand.strength == highHand)
                winner.Add(p);
        }

        if (winner.Count > 1)
        {
            winner = breakTie(winner);
        }

        if (winner.Count == 1)
        {
            Debug.Log("Player " + winner[0].GetComponent<PlayerScript>().playerNumber + " Wins");
            winner[0].GetComponent<PlayerScript>().hand.printResult();
        }
    }

    public List<GameObject> breakTie(List<GameObject> player)
    {
        int highestRank, playerRank;
        List<GameObject> winningPlayer = player;

        int bounds = player[0].GetComponent<PlayerScript>().hand.highCard.Count;

        for (int i = 0; i < bounds; i++)
        {
            List<GameObject> tmp = winningPlayer;
            highestRank = 0;

            // for each card find the highest
            foreach (GameObject p in tmp)
            {
                playerRank = (int)p.GetComponent<PlayerScript>().hand.highCard[i].value;

                // find the player with the highest ranking cards
                if (playerRank > highestRank)
                {
                    highestRank = playerRank;
                }
            }

            // create a new list of the winniest players
            winningPlayer = new List<GameObject>();
            foreach (GameObject p in tmp)
            {
                playerRank = (int)p.GetComponent<PlayerScript>().hand.highCard[i].value;

                // eliminate players with crappy cards
                if (playerRank >= highestRank)
                {
                    winningPlayer.Add(p);
                }
            }

            if (winningPlayer.Count == 1)
                return winningPlayer;
        }
        return winningPlayer;
    }
}
