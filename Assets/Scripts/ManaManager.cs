using UnityEngine;
using System;

public class ManaManager : MonoBehaviour
{
    [Header("Réglages des Charges")]
    [Tooltip("Nombre de charges maximum par défaut")]
    [SerializeField] private int maxCharges = 2;
    [Tooltip("Temps en secondes pour recharger UNE charge")]
    [SerializeField] private float timeToRecharge = 30f;

    // Propriété publique pour lire le nombre de charges actuelles
    public int CurrentCharges { get; private set; }

    // Chronomètre interne
    private float rechargeTimer = 0f;

    // Événements optionnels pour mettre à jour ton UI plus tard
    public event Action<int, int> OnManaChanged;
    public event Action<float> OnRechargeProgressChanged;

    void Start()
    {
        CurrentCharges = maxCharges;
        OnManaChanged?.Invoke(CurrentCharges, maxCharges);
    }

    void Update()
    {
        // Si on n'est pas au maximum de nos charges, le chrono tourne
        if (CurrentCharges < maxCharges)
        {
            rechargeTimer += Time.deltaTime;

            // On envoie la progression (de 0 à 1) pour une éventuelle barre de chargement UI
            OnRechargeProgressChanged?.Invoke(rechargeTimer / timeToRecharge);

            // Quand le chrono atteint 30 secondes...
            if (rechargeTimer >= timeToRecharge)
            {
                CurrentCharges++; // On gagne une charge !
                rechargeTimer -= timeToRecharge; // On réinitialise le chrono (en gardant le surplus éventuel)

                OnManaChanged?.Invoke(CurrentCharges, maxCharges);
            }
        }
        else
        {
            // Si on est full, on maintient le chrono à zéro
            rechargeTimer = 0f;
            OnRechargeProgressChanged?.Invoke(1f);
        }
    }

    // --- FONCTIONS PUBLIQUES (Pour les autres scripts) ---

    // 1. Tenter de consommer de la mana (appelé par le sort)
    public bool TryConsumeCharge(int cost)
    {
        if (CurrentCharges >= cost)
        {
            CurrentCharges -= cost;
            OnManaChanged?.Invoke(CurrentCharges, maxCharges);
            return true; // Le sort est autorisé !
        }
        return false; // Pas assez de charges
    }

    // 2. Ramasser un objet qui recharge un pourcentage (ex: 0.5f pour 50%)
    public void RechargePercentage(float percentage)
    {
        if (CurrentCharges >= maxCharges) return; // Déjà au max

        // On avance le chronomètre du pourcentage donné
        rechargeTimer += (timeToRecharge * percentage);

        // Si ce gain nous fait dépasser les 30 secondes, on valide la ou les charges gagnées
        while (rechargeTimer >= timeToRecharge && CurrentCharges < maxCharges)
        {
            CurrentCharges++;
            rechargeTimer -= timeToRecharge;
            OnManaChanged?.Invoke(CurrentCharges, maxCharges);
        }

        // Sécurité si on a atteint le max
        if (CurrentCharges >= maxCharges) rechargeTimer = 0f;
    }

    // 3. Ramasser un objet qui rend une ou plusieurs charges complètes instantanément
    public void AddFullCharge(int amount = 1)
    {
        CurrentCharges += amount;
        if (CurrentCharges > maxCharges)
        {
            CurrentCharges = maxCharges;
        }
        OnManaChanged?.Invoke(CurrentCharges, maxCharges);
    }

    // 4. Améliorer son personnage de façon permanente (ex: passer à 3 charges max)
    public void IncreaseMaxCharges(int amount)
    {
        maxCharges += amount;
        CurrentCharges += amount; // On lui offre la charge qu'il vient de débloquer
        OnManaChanged?.Invoke(CurrentCharges, maxCharges);
    }
}
