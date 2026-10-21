using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public Hero hero;
    public CombatManager combat;

    public List<HandSkill> hand = new List<HandSkill>();
    public int maxHandSize = 4;

    public int actionsLeft = 1;
    public Transform dragLayer;
    public Transform handPanel;
    public Transform inactivePool;
    public GameObject cardPrefab;
    public HeroSkillEventDispatcher skillevent;
    public bool merging = false;
    public bool rankupMode = false;
    public int rankupLevel = 1;      // Nivel de rankup (1,2,3)
    public int rankupRemaining = 0;  // CuÃ¡ntas cartas faltan por subir

    public void RefreshHandUI()
    {
        if (combat != null) skillevent = combat.currentDispatcher;
        if (combat != null && combat.skillButtons != null && combat.skillButtons.Length > 0 && combat.skillButtons[0] != null)
        {
            var anim = combat.skillButtons[0].GetComponent<CardAnimatorControllerDOTween>();
            if (anim != null)
            {
                foreach (var card in hand)
                {
                    anim.ForceRecalculateBase();
                }
            }
        }

        // Si el jugador estÃ¡ arrastrando una carta, NO refrescamos todavÃ­a
        if (CardDragHandler.IsDragging)
            return;

        // 1. Crear objetos suficientes en el pool (pero NO activarlos)
        while (inactivePool.childCount + handPanel.childCount < maxHandSize)
        {
            var obj = Instantiate(cardPrefab);
            obj.transform.SetParent(inactivePool, false);
            obj.SetActive(false);
        }

        // 2. Mover cartas sobrantes al pool (sin activar nada)
        while (handPanel.childCount > hand.Count)
        {
            var obj = handPanel.GetChild(handPanel.childCount - 1).gameObject;

            var drag = obj.GetComponent<CardDragHandler>();
            if (drag != null && drag.isDraggingThis)
                break;

            obj.transform.SetParent(inactivePool, false);
            obj.SetActive(false);
        }

        // 3. Activar SOLO las necesarias, una por una, sin activar todas
        while (handPanel.childCount < hand.Count)
        {
            // NO crear cartas mientras se estÃ© arrastrando o justo al soltar
            if (CardDragHandler.IsDragging)
                break;

            var obj = inactivePool.GetChild(0).gameObject;
            obj.transform.SetParent(handPanel, false);
            obj.SetActive(true);
        }

        // Actualizar cada carta
        for (int i = 0; i < Mathf.Min(hand.Count, handPanel.childCount); i++)
        {
            // Seguridad: si la carta es null, saltamos
            if (hand[i] == null || hand[i].baseSkill == null)
                continue;

            var cardObj = handPanel.GetChild(i).gameObject;
            var ui = cardObj.GetComponent<CardUI>();

            ui.cardIndex = i;
            ui.handManager = this;

            var drag = cardObj.GetComponent<CardDragHandler>();
            var drop = cardObj.GetComponent<CardDropHandler>();

            if (drag != null) drag.cardIndex = i;
            if (drop != null)
            {
                drop.cardIndex = i;
                drop.handManager = this;
            }

            ui.UpdateVisual(hand[i]);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(handPanel as RectTransform);
    }

    public void GenerateInitialHand()
    {
        hand.Clear();
        for (int i = 0; i < maxHandSize; i++)
            hand.Add(new HandSkill(GetRandomSkill()));
    }

    public void StartTurn()
    {
        actionsLeft = 1;

        while (hand.Count < maxHandSize)
            hand.Add(new HandSkill(GetRandomSkill()));

        RefreshHandUI();
    }

    private HeroSkill GetRandomSkill()
    {
        int r = Random.Range(0, hero.skills.Count);
        return hero.skills[r];
    }

    public void UseSkill(int index)
    {
        if (combat != null) skillevent = combat.currentDispatcher;
        if (actionsLeft <= 0) return;
        if (index < 0 || index >= hand.Count) return;

        var skill = hand[index];
        if (skill == null || skill.baseSkill == null) return;

        // Si estamos en modo Rankup, NO usamos la carta la mejoramos
        if (rankupMode)
        {
            var s = hand[index];

            if (s.tier < 3)
            {
                s.tier++;
                rankupRemaining--;
                combat.dialogue.text = $"Â¡{s.baseSkill.skillName} sube a Tier {s.tier}! ({rankupRemaining} restantes)";
            }
            else
            {
                combat.dialogue.text = $"{s.baseSkill.skillName} ya estÃ¡ al mÃ¡ximo Tier.";
            }

            RefreshHandUI();

            // Â¿Ya terminamos?
            if (rankupRemaining <= 0)
            {
                rankupMode = false;
                DisableRankupFeedback();

                for (int i = 0; i < 4; i++)
                {
                    if (combat != null && combat.skillButtons != null && i < combat.skillButtons.Length && combat.skillButtons[i] != null)
                    {
                        var shine = combat.skillButtons[i].GetComponent<ShineDOTween>();
                        if (shine != null) shine.DesactivarEfectos();
                    }
                }

                combat.dialogue.text = "Â¡Rankup completado!";
            }

            return;
        }

        // Modo normal: usar carta
        if (actionsLeft <= 0) return;

        actionsLeft--;

        // Ejecutar skill en combate
        // 🔧 FIX: combat.PlayerUseSkillFromHand ya dispara internamente
        // currentDispatcher.TriggerSkillEvent(...) (ver CombatManager.
        // PlayerUseSkillFromHandRoutine). La llamada manual que había aquí
        // duplicaba el evento (doble daño/doble efecto visual por cada uso
        // de skill) y además usaba `skillevent` cacheado al inicio del
        // método, evitando la resolución robusta que sí hace CombatManager.
        combat.PlayerUseSkillFromHand(skill.baseSkill, skill.tier);

        // Disparar eventos de skills otorgadas por equipo
        var eqDispatcher = FindObjectOfType<EquipmentSkillEventDispatcher>();
        if (eqDispatcher != null)
            eqDispatcher.OnHeroUsedSkill(skill.baseSkill, skill.tier);

        // Eliminar carta usada
        hand.RemoveAt(index);

        RefreshHandUI();
    }

    public void MoveCardInternal(int from, int to)
    {
        if (from < 0 || from >= hand.Count) return;

        if (to < 0) to = 0;
        if (to >= hand.Count) to = hand.Count - 1;

        var card = hand[from];

        if (from < to)
        {
            for (int i = from; i < to; i++)
                hand[i] = hand[i + 1];
        }
        else if (from > to)
        {
            for (int i = from; i > to; i--)
                hand[i] = hand[i - 1];
        }

        hand[to] = card;
    }

    public void TryManualMerge(int index)
    {
        merging = true;
        if (index < 0 || index >= hand.Count - 1) return;

        var a = hand[index];
        var b = hand[index + 1];

        if (a == null || b == null) return;

        if (a.baseSkill == b.baseSkill && a.tier == b.tier && a.tier < 3)
        {
            a.tier++;
            for (int i = 0; i < 4; i++)
            {
                if (combat != null && combat.skillButtons != null && i < combat.skillButtons.Length && combat.skillButtons[i] != null)
                {
                    var anim = combat.skillButtons[i].GetComponent<CardAnimatorControllerDOTween>();
                    if (anim != null) anim.PlayMergeAnimation();
                }
            }
            hand[index + 1] = null;

            CompactHand();   // OBLIGATORIO
        }
    }

    private void CompactHand()
    {
        for (int i = 0; i < Mathf.Min(hand.Count, handPanel.childCount); i++)
        {
            if (hand[i] == null)
            {
                for (int j = i + 1; j < hand.Count; j++)
                {
                    if (hand[j] != null)
                    {
                        hand[i] = hand[j];
                        hand[j] = null;
                        break;
                    }
                }
            }
        }

        hand.RemoveAll(h => h == null);
    }

    public float ApplyTierMultiplier(float damage, int tier)
    {
        return tier == 1 ? damage :
               tier == 2 ? damage * 1.5f :
                           damage * 2f;
    }

    public void EnableRankupFeedback()
    {
        for (int i = 0; i < hand.Count - 1; i++)
        {
            var a = hand[i];
            var b = hand[i + 1];

            if (a != null && b != null &&
                a.baseSkill == b.baseSkill &&
                a.tier == b.tier &&
                a.tier < 3)
            {
                var cardObj = handPanel.GetChild(i).GetComponent<RankupFeedback>();
                cardObj?.PlayFeedback();

                var cardObj2 = handPanel.GetChild(i + 1).GetComponent<RankupFeedback>();
                cardObj2?.PlayFeedback();
            }
        }
    }

    public void DisableRankupFeedback()
    {
        foreach (Transform t in handPanel)
        {
            var fx = t.GetComponent<RankupFeedback>();
            fx?.StopFeedback();
        }
    }
}